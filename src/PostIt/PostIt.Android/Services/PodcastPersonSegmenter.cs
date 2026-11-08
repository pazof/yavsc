using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Microsoft.ML.OnnxRuntime;

namespace PostIt.Android.Services;

internal interface IPodcastPersonSegmenter : IDisposable
{
    byte[] Segment(byte[] rgba);
}

internal sealed class PodcastPersonSegmenter : IPodcastPersonSegmenter
{
    internal const int Size = 192;
    private const int Pixels = Size * Size;
    private readonly InferenceSession _session;
    private readonly RunOptions _runOptions = new();
    private readonly float[] _input = new float[3 * Pixels];
    private readonly byte[] _mask = new byte[Pixels];
    private readonly string _inputName;

    internal PodcastPersonSegmenter(Context context)
    {
        using var asset = context.Assets?.Open("PodcastBackground/pphumanseg.onnx")
            ?? throw new InvalidOperationException("Modele de segmentation Android introuvable.");
        using var model = new MemoryStream();
        asset.CopyTo(model);
        using var options = new SessionOptions { IntraOpNumThreads = 2, InterOpNumThreads = 1 };
        _session = new InferenceSession(model.ToArray(), options);
        _inputName = _session.InputNames.Single();
    }

    public byte[] Segment(byte[] rgba)
    {
        if (rgba.Length != Pixels * 4) throw new ArgumentException("Image de segmentation invalide.", nameof(rgba));
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                var pixel = y * Size + x;
                var source = ((Size - 1 - y) * Size + x) * 4;
                _input[pixel] = rgba[source] / 127.5f - 1;
                _input[Pixels + pixel] = rgba[source + 1] / 127.5f - 1;
                _input[2 * Pixels + pixel] = rgba[source + 2] / 127.5f - 1;
            }
        }
        using var input = OrtValue.CreateTensorValueFromMemory(_input, [1, 3, Size, Size]);
        using var outputs = _session.Run(_runOptions, new Dictionary<string, OrtValue> { [_inputName] = input }, _session.OutputNames);
        var result = outputs[0].GetTensorDataAsSpan<float>();
        if (result.Length != 2 * Pixels) throw new InvalidDataException("Sortie du modele de segmentation invalide.");
        for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                var pixel = y * Size + x;
                var background = result[pixel];
                var person = result[Pixels + pixel];
                if (!float.IsFinite(background) || !float.IsFinite(person))
                    throw new InvalidDataException("Sortie du modele de segmentation non finie.");
                _mask[(Size - 1 - y) * Size + x] = person > background ? (byte)255 : (byte)0;
            }
        return _mask;
    }

    public void Dispose()
    {
        _session.Dispose();
        _runOptions.Dispose();
    }
}
