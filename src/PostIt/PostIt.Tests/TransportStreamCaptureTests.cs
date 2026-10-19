using PostIt.Services;

namespace PostIt.Tests;

public sealed class TransportStreamCaptureTests
{
    [Fact]
    public async Task Segments_on_IDR_and_flushes_final_segment_with_real_PTS_durations()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"postit-ts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            using var input = new MemoryStream();
            input.Write(Pat());
            input.Write(Pmt());
            for (var i = 0; i <= 120; i++) input.Write(Video(i * 3000L, i % 60 == 0));
            input.Position = 0;
            await new TransportStreamCapture(directory).RunAsync(input, TestContext.Current.CancellationToken);
            var segments = HlsCapturePlaylist.Parse(await File.ReadAllTextAsync(Path.Combine(directory, "index.m3u8"), TestContext.Current.CancellationToken));
            Assert.Equal(3, segments.Count);
            Assert.Equal(2, segments[0].DurationSeconds);
            Assert.Equal(2, segments[1].DurationSeconds);
            Assert.InRange(segments[2].DurationSeconds, 0.033, 0.034);
            foreach (var segment in segments)
            {
                var bytes = await File.ReadAllBytesAsync(Path.Combine(directory, segment.FileName), TestContext.Current.CancellationToken);
                Assert.Equal(0, bytes.Length % 188);
                Assert.Equal(Pat(), bytes[..188]);
                Assert.Equal(Pmt(), bytes[188..376]);
                Assert.Contains((byte)0x65, bytes[376..]);
            }
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Rejects_corrupt_or_truncated_transport_without_publishing_a_segment(bool truncated)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"postit-ts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            using var input = new MemoryStream(new byte[truncated ? 187 : 188]);
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                new TransportStreamCapture(directory).RunAsync(input, TestContext.Current.CancellationToken));
            Assert.False(File.Exists(Path.Combine(directory, "index.m3u8")));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public async Task Preserves_out_of_band_H264_configuration_at_every_segment_start()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"postit-ts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            using var input = new MemoryStream();
            input.Write(Pat());
            input.Write(Pmt());
            var configuration = Video(0, false);
            configuration[22] = 0x67;
            new byte[] { 0, 0, 0, 1, 0x68, 0x80 }.CopyTo(configuration, 25);
            input.Write(configuration);
            for (var i = 0; i <= 120; i++) input.Write(Video(90000 + i * 3000L, i % 60 == 0));
            input.Position = 0;
            await new TransportStreamCapture(directory).RunAsync(input, TestContext.Current.CancellationToken);
            var segments = HlsCapturePlaylist.Parse(await File.ReadAllTextAsync(Path.Combine(directory, "index.m3u8"), TestContext.Current.CancellationToken));
            Assert.Equal(3, segments.Count);
            foreach (var segment in segments)
            {
                var bytes = await File.ReadAllBytesAsync(Path.Combine(directory, segment.FileName), TestContext.Current.CancellationToken);
                Assert.Equal(0x67, bytes[376 + 22]);
                Assert.Equal(0x68, bytes[376 + 29]);
                Assert.Equal(0x65, bytes[564 + 22]);
                Assert.Equal((bytes[564 + 3] - 1) & 0x0f, bytes[376 + 3] & 0x0f);
                Assert.Equal(bytes[(564 + 13)..(564 + 18)], bytes[(376 + 13)..(376 + 18)]);
            }
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public async Task Handles_33_bit_PTS_wrap_without_negative_duration()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"postit-ts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            using var input = new MemoryStream();
            input.Write(Pat());
            input.Write(Pmt());
            for (var i = 0; i <= 120; i++)
                input.Write(Video(((1L << 33) - 90000 + i * 3000L) & ((1L << 33) - 1), i % 60 == 0));
            input.Position = 0;
            await new TransportStreamCapture(directory).RunAsync(input, TestContext.Current.CancellationToken);
            var segments = HlsCapturePlaylist.Parse(await File.ReadAllTextAsync(Path.Combine(directory, "index.m3u8"), TestContext.Current.CancellationToken));
            Assert.Equal(3, segments.Count);
            Assert.Equal(2, segments[0].DurationSeconds);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    private static byte[] Packet(int pid)
    {
        var packet = Enumerable.Repeat((byte)0xff, 188).ToArray();
        packet[0] = 0x47;
        packet[1] = (byte)(0x40 | (pid >> 8));
        packet[2] = (byte)pid;
        packet[3] = 0x10;
        return packet;
    }

    private static byte[] Pat()
    {
        var packet = Packet(0);
        packet[4] = 0;
        new byte[] { 0, 0xb0, 13, 0, 1, 0xc1, 0, 0, 0, 1, 0xe1, 0 }.CopyTo(packet, 5);
        return packet;
    }

    private static byte[] Pmt()
    {
        var packet = Packet(256);
        packet[4] = 0;
        new byte[] { 2, 0xb0, 18, 0, 1, 0xc1, 0, 0, 0xe1, 1, 0xf0, 0,
            0x1b, 0xe1, 1, 0xf0, 0 }.CopyTo(packet, 5);
        return packet;
    }

    private static byte[] Video(long pts, bool idr)
    {
        var packet = Packet(257);
        new byte[] { 0, 0, 1, 0xe0, 0, 0, 0x84, 0x80, 5 }.CopyTo(packet, 4);
        packet[13] = (byte)(0x21 | ((pts >> 29) & 0xe));
        packet[14] = (byte)(pts >> 22);
        packet[15] = (byte)(1 | ((pts >> 14) & 0xfe));
        packet[16] = (byte)(pts >> 7);
        packet[17] = (byte)(1 | ((pts << 1) & 0xfe));
        new byte[] { 0, 0, 0, 1, idr ? (byte)0x65 : (byte)0x41, 0x88 }.CopyTo(packet, 18);
        return packet;
    }
}
