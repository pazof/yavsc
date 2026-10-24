using System;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Opengl;
using Android.OS;
using Android.Views;
using Java.Nio;

namespace PostIt.Android.Services;

internal sealed class AndroidCameraFrameRotator : IDisposable
{
    private const int ExternalTexture = 0x8D65;
    private const int RecordableAndroid = 0x3142;
    private const string VertexShader = """
        attribute vec2 position;
        attribute vec2 textureCoordinate;
        uniform mat4 textureMatrix;
        varying vec2 uv;
        void main() {
            gl_Position = vec4(position, 0.0, 1.0);
            uv = (textureMatrix * vec4(textureCoordinate, 0.0, 1.0)).xy;
        }
        """;
    private const string FragmentShader = """
        #extension GL_OES_EGL_image_external : require
        precision mediump float;
        uniform samplerExternalOES cameraTexture;
        varying vec2 uv;
        void main() {
            gl_FragColor = texture2D(cameraTexture, uv);
        }
        """;

    private readonly HandlerThread _thread = new("PostIt.CameraRotation");
    private readonly Handler _handler;
    private readonly Action<Exception> _failed;
    private readonly float[] _textureMatrix = new float[16];
    private readonly CameraFrameClock _clock = new();
    private EGLDisplay? _display;
    private EGLContext? _context;
    private EGLSurface? _window;
    private SurfaceTexture? _texture;
    private FrameListener? _listener;
    private Surface? _cameraSurface;
    private FloatBuffer? _vertices;
    private FloatBuffer? _coordinates;
    private int _program;
    private int _textureId;
    private int _matrixLocation;
    private bool _rendering;
    private bool _disposed;
    private int _rotation;
    private int? _remainingRotation;
    internal string? FirstTextureTransform { get; private set; }

    internal Surface CameraSurface => _cameraSurface
        ?? throw new InvalidOperationException("Surface de rotation camera indisponible.");

    private AndroidCameraFrameRotator(Action<Exception> failed)
    {
        _failed = failed;
        _thread.Start();
        _handler = new Handler(_thread.Looper!);
    }

    internal static async Task<AndroidCameraFrameRotator> CreateAsync(
        Surface encoderSurface, int width, int height, int rotation, Action<Exception> failed)
    {
        var rotator = new AndroidCameraFrameRotator(failed);
        try
        {
            await rotator.InvokeAsync(() => rotator.Initialize(encoderSurface, width, height, rotation));
            return rotator;
        }
        catch
        {
            rotator.Dispose();
            throw;
        }
    }

    private void Initialize(Surface encoderSurface, int width, int height, int rotation)
    {
        _rotation = rotation;
        _display = EGL14.EglGetDisplay(EGL14.EglDefaultDisplay);
        if (_display is null || _display.Equals(EGL14.EglNoDisplay))
            throw EglError("Obtention de l'affichage EGL");
        if (!EGL14.EglInitialize(_display, new int[2], 0, new int[2], 0))
            throw EglError("Initialisation EGL");
        int[] attributes =
        [
            EGL14.EglRedSize, 8, EGL14.EglGreenSize, 8, EGL14.EglBlueSize, 8,
            EGL14.EglRenderableType, EGL14.EglOpenglEs2Bit,
            EGL14.EglSurfaceType, EGL14.EglWindowBit, RecordableAndroid, 1, EGL14.EglNone
        ];
        var configs = new EGLConfig[1];
        var count = new int[1];
        if (!EGL14.EglChooseConfig(_display, attributes, 0, configs, 0, 1, count, 0) || count[0] == 0)
            throw EglError("Configuration EGL pour l'encodeur");
        _context = EGL14.EglCreateContext(_display, configs[0], EGL14.EglNoContext,
            [EGL14.EglContextClientVersion, 2, EGL14.EglNone], 0);
        if (_context is null || _context.Equals(EGL14.EglNoContext))
            throw EglError("Creation du contexte EGL");
        _window = EGL14.EglCreateWindowSurface(_display, configs[0], encoderSurface, [EGL14.EglNone], 0);
        if (_window is null || _window.Equals(EGL14.EglNoSurface))
            throw EglError("Creation de la surface EGL");
        if (!EGL14.EglMakeCurrent(_display, _window, _window, _context))
            throw EglError("Activation du contexte EGL");

        _program = CreateProgram();
        var ids = new int[1];
        GLES20.GlGenTextures(1, ids, 0);
        _textureId = ids[0];
        GLES20.GlBindTexture(ExternalTexture, _textureId);
        GLES20.GlTexParameteri(ExternalTexture, GLES20.GlTextureMinFilter, GLES20.GlLinear);
        GLES20.GlTexParameteri(ExternalTexture, GLES20.GlTextureMagFilter, GLES20.GlLinear);
        GLES20.GlTexParameteri(ExternalTexture, GLES20.GlTextureWrapS, GLES20.GlClampToEdge);
        GLES20.GlTexParameteri(ExternalTexture, GLES20.GlTextureWrapT, GLES20.GlClampToEdge);
        _vertices = CreateBuffer([-1, -1, 1, -1, -1, 1, 1, 1]);
        _coordinates = CreateBuffer(CameraFrameOrientation.GetTextureCoordinates(0));
        GLES20.GlUseProgram(_program);
        BindAttribute("position", _vertices);
        BindAttribute("textureCoordinate", _coordinates);
        _matrixLocation = GLES20.GlGetUniformLocation(_program, "textureMatrix");
        GLES20.GlUniform1i(GLES20.GlGetUniformLocation(_program, "cameraTexture"), 0);
        GLES20.GlViewport(0, 0, rotation is 90 or 270 ? height : width, rotation is 90 or 270 ? width : height);
        CheckGl();

        _texture = new SurfaceTexture(_textureId);
        _texture.SetDefaultBufferSize(width, height);
        _listener = new FrameListener(this);
        _texture.SetOnFrameAvailableListener(_listener, _handler);
        _cameraSurface = new Surface(_texture);
    }

    internal Task StartAsync() => InvokeAsync(() => _rendering = true);

    private void DrawFrame(SurfaceTexture texture)
    {
        if (!_rendering) return;
        try
        {
            texture.UpdateTexImage();
            var presentationTime = _clock.GetPresentationTime(texture.Timestamp, Java.Lang.JavaSystem.NanoTime());
            if (presentationTime is null) return;
            texture.GetTransformMatrix(_textureMatrix);
            var remainingRotation = CameraFrameOrientation.GetRemainingRotation(_rotation, _textureMatrix);
            if (_remainingRotation != remainingRotation)
            {
                _remainingRotation = remainingRotation;
                FirstTextureTransform = string.Join(", ", _textureMatrix);
                _coordinates!.Position(0);
                _coordinates.Put(CameraFrameOrientation.GetTextureCoordinates(remainingRotation));
                _coordinates.Position(0);
                global::Android.Util.Log.Info("PostIt.Podcast",
                    $"Camera texture transform: [{FirstTextureTransform}], remaining rotation={remainingRotation}");
            }
            GLES20.GlUniformMatrix4fv(_matrixLocation, 1, false, _textureMatrix, 0);
            GLES20.GlDrawArrays(GLES20.GlTriangleStrip, 0, 4);
            CheckGl();
            if (!EGLExt.EglPresentationTimeANDROID(_display, _window, presentationTime.Value))
                throw EglError("Horodatage de l'image camera");
            if (!EGL14.EglSwapBuffers(_display, _window))
                throw EglError("Envoi de l'image a l'encodeur");
        }
        catch (Exception ex)
        {
            _rendering = false;
            _failed(new InvalidOperationException("Rotation de la capture Android impossible.", ex));
        }
    }

    private Task InvokeAsync(Action action)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_handler.Post(() =>
        {
            try { action(); completion.SetResult(); }
            catch (Exception ex) { completion.SetException(ex); }
        }))
            completion.SetException(new InvalidOperationException("Le thread de rotation camera est arrete."));
        return completion.Task;
    }

    private static FloatBuffer CreateBuffer(float[] values)
    {
        using var bytes = ByteBuffer.AllocateDirect(values.Length * sizeof(float))
            ?? throw new InvalidOperationException("Allocation du tampon OpenGL impossible.");
        bytes.Order(ByteOrder.NativeOrder()!);
        var buffer = bytes.AsFloatBuffer()
            ?? throw new InvalidOperationException("Creation du tampon OpenGL impossible.");
        buffer.Put(values);
        buffer.Position(0);
        return buffer;
    }

    private void BindAttribute(string name, FloatBuffer buffer)
    {
        var location = GLES20.GlGetAttribLocation(_program, name);
        GLES20.GlEnableVertexAttribArray(location);
        GLES20.GlVertexAttribPointer(location, 2, GLES20.GlFloat, false, 0, buffer);
    }

    private static int CreateShader(int type, string source)
    {
        var shader = GLES20.GlCreateShader(type);
        GLES20.GlShaderSource(shader, source);
        GLES20.GlCompileShader(shader);
        var status = new int[1];
        GLES20.GlGetShaderiv(shader, GLES20.GlCompileStatus, status, 0);
        if (status[0] != 0) return shader;
        var error = GLES20.GlGetShaderInfoLog(shader);
        GLES20.GlDeleteShader(shader);
        throw new InvalidOperationException($"Compilation du shader camera impossible : {error}");
    }

    private static int CreateProgram()
    {
        var vertex = CreateShader(GLES20.GlVertexShader, VertexShader);
        var fragment = 0;
        var program = 0;
        try
        {
            fragment = CreateShader(GLES20.GlFragmentShader, FragmentShader);
            program = GLES20.GlCreateProgram();
            GLES20.GlAttachShader(program, vertex);
            GLES20.GlAttachShader(program, fragment);
            GLES20.GlLinkProgram(program);
            var status = new int[1];
            GLES20.GlGetProgramiv(program, GLES20.GlLinkStatus, status, 0);
            if (status[0] == 0)
                throw new InvalidOperationException($"Edition du programme camera impossible : {GLES20.GlGetProgramInfoLog(program)}");
            return program;
        }
        catch
        {
            if (program != 0) GLES20.GlDeleteProgram(program);
            throw;
        }
        finally
        {
            GLES20.GlDeleteShader(vertex);
            if (fragment != 0) GLES20.GlDeleteShader(fragment);
        }
    }

    private static void CheckGl()
    {
        var error = GLES20.GlGetError();
        if (error != GLES20.GlNoError)
            throw new InvalidOperationException($"Erreur OpenGL de capture : 0x{error:X}.");
    }

    private static InvalidOperationException EglError(string operation)
        => new($"{operation} impossible : EGL 0x{EGL14.EglGetError():X}.");

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { InvokeAsync(Release).GetAwaiter().GetResult(); }
        finally
        {
            _thread.QuitSafely();
            _thread.Join();
            _handler.Dispose();
            _thread.Dispose();
        }
    }

    private void Release()
    {
        _rendering = false;
        _texture?.SetOnFrameAvailableListener(null);
        _cameraSurface?.Release();
        _cameraSurface?.Dispose();
        _texture?.Release();
        _texture?.Dispose();
        _listener?.Dispose();
        if (_program != 0) GLES20.GlDeleteProgram(_program);
        if (_textureId != 0) GLES20.GlDeleteTextures(1, [_textureId], 0);
        _vertices?.Dispose();
        _coordinates?.Dispose();
        if (_display is not null && !_display.Equals(EGL14.EglNoDisplay))
        {
            EGL14.EglMakeCurrent(_display, EGL14.EglNoSurface, EGL14.EglNoSurface, EGL14.EglNoContext);
            if (_window is not null && !_window.Equals(EGL14.EglNoSurface))
                EGL14.EglDestroySurface(_display, _window);
            if (_context is not null && !_context.Equals(EGL14.EglNoContext))
                EGL14.EglDestroyContext(_display, _context);
            EGL14.EglReleaseThread();
            EGL14.EglTerminate(_display);
        }
        _window?.Dispose();
        _context?.Dispose();
        _display?.Dispose();
    }

    private sealed class FrameListener(AndroidCameraFrameRotator owner)
        : Java.Lang.Object, SurfaceTexture.IOnFrameAvailableListener
    {
        public void OnFrameAvailable(SurfaceTexture? texture)
        {
            if (texture is not null) owner.DrawFrame(texture);
        }
    }
}
