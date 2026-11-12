using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using LibVLCSharp.Shared;
using PostIt.Services;

namespace PostIt.Tests;

[Trait("DependsOn", "Native")]
[Trait("Speed", "Slow")]
[Trait("Category", "non-reg")]
public sealed class LinuxLanCallMediaTests(ITestOutputHelper output)
{
    public static bool HasFixture => OperatingSystem.IsLinux() &&
        File.Exists(Environment.GetEnvironmentVariable("POSTIT_TEST_MEDIA"));
    public static bool HasTsFixture => OperatingSystem.IsLinux() &&
        File.Exists(Environment.GetEnvironmentVariable("POSTIT_TEST_TS"));

    [Theory(Skip = "Requires Linux LibVLC.", SkipUnless = nameof(HasFixture))]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    public async Task Ports_are_released_and_credentials_regenerated_after_disposal(string address)
    {
        var port = FreePort(address: address);
        var first = new LibVlcLanCallMedia(address, port, headless: true);
        var endpoint = first.Endpoint;
        await first.StartReceivingAsync(TestContext.Current.CancellationToken);
        await first.DisposeAsync();
        await first.DisposeAsync();
        await using var next = new LibVlcLanCallMedia(address, port, headless: true);
        Assert.NotEqual(endpoint.Key, next.Endpoint.Key);
        Assert.NotEqual(endpoint.Salt, next.Endpoint.Salt);
        await next.StartReceivingAsync(TestContext.Current.CancellationToken);
    }

    [Theory(Skip = "Requires Linux LibVLC.", SkipUnless = nameof(HasFixture))]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    public void Occupied_rtcp_port_is_rejected_before_capture_starts(string address)
    {
        var port = FreePort(address: address);
        var ip = IPAddress.Parse(address);
        using var socket = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(ip, port + 1));
        var error = Assert.Throws<SocketException>(() => new LibVlcLanCallMedia(address, port, headless: true));
        Assert.Equal(SocketError.AddressAlreadyInUse, error.SocketErrorCode);
    }

    [Theory(Skip = "Set POSTIT_TEST_TS to an H.264/AAC MPEG-TS file.", SkipUnless = nameof(HasTsFixture))]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    public async Task Already_encoded_capture_is_sent_over_srtp_without_transcoding(string address)
    {
        var ct = TestContext.Current.CancellationToken;
        var firstPort = FreePort(address: address);
        await using var sender = new LibVlcLanCallMedia(address, firstPort, headless: true);
        await using var receiver = new LibVlcLanCallMedia(address, FreePort(firstPort, address), headless: true);
        using var decoded = new DecodedVideo(receiver.Receiver);
        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        await using var source = File.OpenRead(Environment.GetEnvironmentVariable("POSTIT_TEST_TS")!);
        await receiver.StartReceivingAsync(ct);
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var pump = Task.Run(async () =>
        {
            var buffer = new byte[1316];
            while (await source.ReadAsync(buffer, lifetime.Token) is var count && count > 0)
            {
                await pipe.WriteAsync(buffer.AsMemory(0, count), lifetime.Token);
                await Task.Delay(4, lifetime.Token);
            }
        }, ct);
        try
        {
            await sender.StartEncodedSendingAsync(receiver.Endpoint,
                pipe.ClientSafePipeHandle.DangerousGetHandle().ToInt32(), ct);
            var elapsed = Stopwatch.StartNew();
            while (!HasMedia(receiver, decoded) && elapsed.Elapsed < TimeSpan.FromSeconds(20))
            {
                if (sender.Completion.IsCompleted) await sender.Completion;
                if (receiver.Completion.IsCompleted) await receiver.Completion;
                await Task.Delay(100, ct);
            }
            Assert.True(HasMedia(receiver, decoded));
        }
        finally
        {
            lifetime.Cancel();
            try { await pump; }
            catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
            await sender.StopAsync();
        }
    }

    [Theory(Skip = "Set POSTIT_TEST_MEDIA to an audio/video file; requires Linux LibVLC SRTP and H.264/AAC encoders.",
        SkipUnless = nameof(HasFixture))]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false, "::1")]
    [InlineData(true, false, "::1")]
    public async Task Two_peers_decode_bidirectional_encrypted_audio_video_and_reject_wrong_keys(
        bool wrongKey, bool preview, string address = "127.0.0.1")
    {
        var ct = TestContext.Current.CancellationToken;
        var firstPort = FreePort(address: address);
        var secondPort = FreePort(firstPort, address);
        await using var first = new LibVlcLanCallMedia(address, firstPort, headless: true);
        await using var second = new LibVlcLanCallMedia(address, secondPort, headless: true);
        using var firstVideo = new DecodedVideo(first.Receiver);
        using var secondVideo = new DecodedVideo(second.Receiver);
        await first.StartReceivingAsync(ct);
        await second.StartReceivingAsync(ct);
        await Task.Delay(300, ct);
        Media Source(LibVLC vlc)
        {
            var media = new Media(vlc, new Uri(Path.GetFullPath(Environment.GetEnvironmentVariable("POSTIT_TEST_MEDIA")!)));
            media.AddOption(":input-repeat=5");
            return media;
        }
        await first.StartSendingAsync(wrongKey ? second.Endpoint with { Key = new string('0', 32) } : second.Endpoint,
            Source, preview, ct);
        await second.StartSendingAsync(first.Endpoint, Source, preview, ct);
        var elapsed = Stopwatch.StartNew();
        while (elapsed.Elapsed < TimeSpan.FromSeconds(wrongKey ? 8 : 20))
        {
            if (first.Completion.IsCompleted) await first.Completion;
            if (second.Completion.IsCompleted) await second.Completion;
            if (!wrongKey && HasMedia(first, firstVideo) && HasMedia(second, secondVideo) &&
                first.HasRemoteMedia && second.HasRemoteMedia) break;
            await Task.Delay(100, ct);
        }
        var a = first.Statistics;
        var b = second.Statistics;
        output.WriteLine($"A: audioSamples={firstVideo.AudioSamples}, rendered={firstVideo.Frames}; " +
            $"B: audioSamples={secondVideo.AudioSamples}, rendered={secondVideo.Frames}; elapsed={elapsed.ElapsedMilliseconds}ms");
        Assert.True(HasMedia(first, firstVideo), "First peer must decode received audio and video.");
        Assert.True(first.HasRemoteMedia, "The UI must detect initial audio/video reception.");
        if (wrongKey)
        {
            Assert.Equal(0, b.DecodedVideo);
            Assert.Equal(0, b.DecodedAudio);
            Assert.Equal(0, secondVideo.Frames);
            Assert.Equal(0, secondVideo.AudioSamples);
            Assert.False(second.HasRemoteMedia);
        }
        else
        {
            Assert.True(HasMedia(second, secondVideo), "Second peer must decode received audio and video.");
            Assert.True(second.HasRemoteMedia);
        }
    }

    private static bool HasMedia(LibVlcLanCallMedia peer, DecodedVideo video) =>
        peer.Statistics.DecodedAudio > 0 && video.AudioSamples > 4800 && video.Frames >= 10;

    private static int FreePort(int except = 0, string address = "127.0.0.1")
    {
        var ip = IPAddress.Parse(address);
        for (var port = 32000; port < 60000; port += 2)
        {
            if (port == except) continue;
            using var socket = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            using var rtcp = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(new IPEndPoint(ip, port));
                rtcp.Bind(new IPEndPoint(ip, port + 1));
                return port;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse) { }
        }
        throw new IOException("No free RTP port available.");
    }

    private sealed class DecodedVideo : IDisposable
    {
        private readonly IntPtr _pixels = Marshal.AllocHGlobal(640 * 480 * 4);
        private int _frames;
        private long _audioSamples;
        public int Frames => Volatile.Read(ref _frames);
        public long AudioSamples => Interlocked.Read(ref _audioSamples);
        private readonly MediaPlayer _player;
        public DecodedVideo(MediaPlayer player)
        {
            _player = player;
            player.SetAudioFormat("S16N", 48000, 1);
            player.SetAudioCallbacks((_, _, count, _) => Interlocked.Add(ref _audioSamples, count),
                null, null, null, null);
            player.SetVideoFormat("RV32", 640, 480, 640 * 4);
            player.SetVideoCallbacks((_, planes) =>
            {
                Marshal.WriteIntPtr(planes, _pixels);
                return IntPtr.Zero;
            }, null, (_, _) => Interlocked.Increment(ref _frames));
        }
        public void Dispose()
        {
            _player.Stop();
            Marshal.FreeHGlobal(_pixels);
        }
    }
}
