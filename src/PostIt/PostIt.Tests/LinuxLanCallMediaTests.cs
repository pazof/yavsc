using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using LibVLCSharp.Shared;
using PostIt.Services;

namespace PostIt.Tests;

public sealed class LinuxLanCallMediaTests(ITestOutputHelper output)
{
    public static bool HasFixture => OperatingSystem.IsLinux() &&
        File.Exists(Environment.GetEnvironmentVariable("POSTIT_TEST_MEDIA"));

    [Theory(Skip = "Set POSTIT_TEST_MEDIA to an audio/video file; requires Linux LibVLC SRTP and H.264/AAC encoders.",
        SkipUnless = nameof(HasFixture))]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Two_peers_decode_bidirectional_encrypted_audio_video_and_reject_wrong_keys(bool wrongKey, bool preview)
    {
        var ct = TestContext.Current.CancellationToken;
        var firstPort = FreePort();
        var secondPort = FreePort(firstPort);
        await using var first = new LinuxLanCallMedia("127.0.0.1", firstPort, headless: true);
        await using var second = new LinuxLanCallMedia("127.0.0.1", secondPort, headless: true);
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

    private static bool HasMedia(LinuxLanCallMedia peer, DecodedVideo video) =>
        peer.Statistics.DecodedAudio > 0 && video.AudioSamples > 4800 && video.Frames >= 10;

    private static int FreePort(int except = 0)
    {
        for (var port = 32000; port < 60000; port += 2)
        {
            if (port == except) continue;
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try { socket.Bind(new IPEndPoint(IPAddress.Loopback, port)); return port; }
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
