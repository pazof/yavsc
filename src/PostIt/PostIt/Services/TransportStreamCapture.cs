using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Services;

/// <summary>Segments a single-program H.264 MPEG-TS encoder output at IDR frames.</summary>
public sealed class TransportStreamCapture(string directory)
{
    private const int PacketSize = 188;
    private const int MaxSegmentBytes = 8 * 1024 * 1024;
    private readonly MemoryStream _frame = new();
    private readonly MemoryStream _video = new();
    private readonly StringBuilder _playlist = new("#EXTM3U\n");
    private byte[]? _pat;
    private byte[]? _pmt;
    private byte[]? _configuration;
    private int _pmtPid = -1;
    private int _videoPid = -1;
    private FileStream? _segment;
    private int _sequence;
    private long? _framePts;
    private long _startPts;
    private long _lastPts;
    private long _frameDuration = 3000;
    private long? _previousPts;
    private long _ptsEpoch;

    public async Task RunAsync(Stream input, CancellationToken ct)
    {
        var packet = new byte[PacketSize];
        try
        {
            while (true)
            {
                var read = 0;
                while (read < PacketSize)
                {
                    var count = await input.ReadAsync(packet.AsMemory(read), ct);
                    if (count == 0)
                    {
                        if (read != 0) throw new InvalidDataException("Paquet MPEG-TS tronque.");
                        FlushFrame();
                        CloseSegment(_lastPts + _frameDuration);
                        return;
                    }
                    read += count;
                }
                Consume(packet);
            }
        }
        finally
        {
            _segment?.Dispose();
            _frame.Dispose();
            _video.Dispose();
        }
    }

    private void Consume(byte[] packet)
    {
        if (packet[0] != 0x47 || (packet[1] & 0x80) != 0 || (packet[3] & 0xc0) != 0)
            throw new InvalidDataException("Flux MPEG-TS invalide.");
        var pid = ((packet[1] & 0x1f) << 8) | packet[2];
        var hasPayload = (packet[3] & 0x10) != 0;
        if (!hasPayload) return;
        var offset = 4;
        if ((packet[3] & 0x20) != 0) offset += 1 + packet[4];
        if (offset >= PacketSize) throw new InvalidDataException("Adaptation MPEG-TS invalide.");
        var start = (packet[1] & 0x40) != 0;
        if (pid == 0 || pid == _pmtPid)
        {
            if (!start) throw new InvalidDataException("Table MPEG-TS multi-paquets non prise en charge.");
            var section = offset + 1 + packet[offset];
            if (section + 3 > PacketSize) throw new InvalidDataException("Table MPEG-TS tronquee.");
            var end = section + 3 + (((packet[section + 1] & 0x0f) << 8) | packet[section + 2]);
            if (end > PacketSize) throw new InvalidDataException("Table MPEG-TS trop grande.");
            if (pid == 0)
            {
                if (packet[section] != 0 || end - section < 16)
                    throw new InvalidDataException("PAT MPEG-TS invalide.");
                _pmtPid = ((packet[section + 10] & 0x1f) << 8) | packet[section + 11];
                _pat = (byte[])packet.Clone();
            }
            else
            {
                if (packet[section] != 2 || end - section < 16)
                    throw new InvalidDataException("PMT MPEG-TS invalide.");
                var entry = section + 12 + (((packet[section + 10] & 0x0f) << 8) | packet[section + 11]);
                while (entry + 5 <= end - 4)
                {
                    if (packet[entry] == 0x1b)
                        _videoPid = ((packet[entry + 1] & 0x1f) << 8) | packet[entry + 2];
                    entry += 5 + (((packet[entry + 3] & 0x0f) << 8) | packet[entry + 4]);
                }
                _pmt = (byte[])packet.Clone();
            }
            return;
        }
        if (pid == _videoPid && start)
        {
            FlushFrame();
            if (offset + 14 > PacketSize || packet[offset] != 0 || packet[offset + 1] != 0
                || packet[offset + 2] != 1 || (packet[offset + 7] & 0x80) == 0)
                throw new InvalidDataException("Horodatage video MPEG-TS absent.");
            var ptsOffset = offset + 9;
            var pts = ((long)(packet[ptsOffset] & 0x0e) << 29)
                | ((long)packet[ptsOffset + 1] << 22) | ((long)(packet[ptsOffset + 2] & 0xfe) << 14)
                | ((long)packet[ptsOffset + 3] << 7) | (uint)(packet[ptsOffset + 4] >> 1);
            if (_previousPts is { } previous && pts < previous)
            {
                if (previous - pts < (1L << 32))
                    throw new InvalidDataException("Horodatages video non croissants.");
                _ptsEpoch += 1L << 33;
            }
            _previousPts = pts;
            _framePts = pts + _ptsEpoch;
            offset += 9 + packet[offset + 8];
            if (offset > PacketSize) throw new InvalidDataException("En-tete PES invalide.");
        }
        if (_framePts is null) return;
        _frame.Write(packet);
        if (pid == _videoPid) _video.Write(packet, offset, PacketSize - offset);
        if (_frame.Length > MaxSegmentBytes)
            throw new IOException("Une image de capture depasse 8 Mio.");
    }

    private void FlushFrame()
    {
        if (_framePts is not { } pts) return;
        var video = _video.GetBuffer().AsSpan(0, (int)_video.Length);
        var idr = false;
        var hasConfiguration = false;
        var hasSlice = false;
        for (var i = 0; i + 3 < video.Length; i++)
            if (video[i] == 0 && video[i + 1] == 0 && video[i + 2] == 1)
            {
                var type = video[i + 3] & 0x1f;
                if (type == 5) idr = true;
                if (type is 7 or 8) hasConfiguration = true;
                if (type is 1 or 5) hasSlice = true;
            }
        if (hasConfiguration && !hasSlice)
        {
            using var packets = new MemoryStream();
            for (var i = 0; i < _frame.Length; i += PacketSize)
            {
                var packet = _frame.GetBuffer().AsSpan(i, PacketSize);
                if ((((packet[1] & 0x1f) << 8) | packet[2]) == _videoPid)
                    packets.Write(packet);
            }
            _configuration = packets.ToArray();
        }
        if (_segment is not null && idr && pts - _startPts >= 180000)
            CloseSegment(pts);
        if (_segment is null && idr)
        {
            if (_pat is null || _pmt is null)
                throw new InvalidDataException("Tables MPEG-TS manquantes.");
            _startPts = pts;
            _segment = File.Create(Path.Combine(directory, SegmentName()));
            _segment.Write(_pat);
            _segment.Write(_pmt);
            if (!hasConfiguration && _configuration is not null) WriteConfiguration(pts);
        }
        if (_segment is not null)
        {
            if (_segment.Length + _frame.Length > MaxSegmentBytes || pts - _startPts > 14 * 90000)
                throw new IOException("Segment trop grand ou absence d'image cle depuis 14 secondes.");
            _segment.Write(_frame.GetBuffer(), 0, (int)_frame.Length);
            if (pts > _lastPts && _lastPts >= _startPts)
                _frameDuration = Math.Min(pts - _lastPts, 9000);
            _lastPts = pts;
        }
        _frame.SetLength(0);
        _video.SetLength(0);
        _framePts = null;
    }

    private string SegmentName() => $"segment-{_sequence:D8}.ts";

    private void WriteConfiguration(long pts)
    {
        var packets = (byte[])_configuration!.Clone();
        var first = _frame.GetBuffer()[3] & 0x0f;
        var counter = (first - packets.Length / PacketSize) & 0x0f;
        for (var i = 0; i < packets.Length; i += PacketSize)
        {
            packets[i + 3] = (byte)((packets[i + 3] & 0xf0) | counter);
            counter = (counter + 1) & 0x0f;
            if ((packets[i + 1] & 0x40) == 0) continue;
            var offset = i + 4;
            if ((packets[i + 3] & 0x20) != 0) offset += packets[offset] + 1;
            offset += 9;
            if (offset + 5 > i + PacketSize) throw new InvalidDataException("Configuration H.264 invalide.");
            packets[offset] = (byte)(0x21 | ((pts >> 29) & 0x0e));
            packets[offset + 1] = (byte)(pts >> 22);
            packets[offset + 2] = (byte)(1 | ((pts >> 14) & 0xfe));
            packets[offset + 3] = (byte)(pts >> 7);
            packets[offset + 4] = (byte)(1 | ((pts << 1) & 0xfe));
        }
        _segment!.Write(packets);
    }

    private void CloseSegment(long endPts)
    {
        if (_segment is null) return;
        _segment.Dispose();
        _segment = null;
        var duration = (endPts - _startPts) / 90000d;
        if (duration <= 0 || duration > 15) throw new InvalidDataException("Duree MPEG-TS invalide.");
        _playlist.Append("#EXTINF:").Append(duration.ToString("0.######", CultureInfo.InvariantCulture))
            .Append(",\n").Append(SegmentName()).Append('\n');
        var index = Path.Combine(directory, "index.m3u8");
        File.WriteAllText(index + ".tmp", _playlist.ToString());
        File.Move(index + ".tmp", index, overwrite: true);
        _sequence++;
    }
}
