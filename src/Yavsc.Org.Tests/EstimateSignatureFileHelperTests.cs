using System;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Server.Helpers;
using Yavsc.Server.Models.FileSystem;

namespace Yavsc.Org.Tests;

/// <summary>
/// Tests for the <see cref="EstimateSignatureFileHelper"/> static
/// helper. Scope is intentionally narrow: the file-naming format,
/// the strokes counter, and the on-disk write path. The controller
/// (authz, db persistence, signalR notification) is out of scope
/// for this commit and will get a dedicated integration test once
/// the Yavsc.Api test project is set up.
/// </summary>
public class EstimateSignatureFileHelperTests : IDisposable
{
    private readonly string _tempRoot;

    public EstimateSignatureFileHelperTests()
    {
        // UserFilesDirName is a process-wide static; we redirect
        // it to a per-test temp dir so concurrent tests don't
        // collide and the host filesystem is not littered.
        _tempRoot = Path.Combine(
            Path.GetTempPath(),
            "yavsc-sig-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
        AbstractFileSystemHelpers.UserFilesDirName = _tempRoot;
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempRoot, recursive: true); }
        catch { /* best effort — the OS will clean Temp eventually */ }
    }

    [Fact]
    public void FileNameFormat_lowercases_type_and_includes_estimateId_and_ticks()
    {
        var name = EstimateSignatureFileHelper.FileNameFormat(
            SignatureType.Pro, 42, 638_000_000_000_000_000L);
        Assert.Equal("sign-pro-42-638000000000000000.json", name);

        var cli = EstimateSignatureFileHelper.FileNameFormat(
            SignatureType.Client, 7, 1L);
        Assert.Equal("sign-client-7-1.json", cli);
    }

    [Theory]
    [InlineData(new int[] { }, 0)]
    [InlineData(new[] { 1, 100, 200 }, 1)]
    [InlineData(new[] { 2, 1, 2, 3, 4 }, 1)]
    [InlineData(new[] { 1, 1, 1, 2, 2, 3, 3 }, 2)]
    [InlineData(new[] { 0, 1, 2, 3 }, 0)] // malformed k=0: short-circuit
    public void ReceiveEstimateSignatureAsync_writes_a_v1_envelope(int[] strokes, int expectedStrokeCount)
    {
        // We don't read the count back from the helper (it's a
        // private method), but the JSON envelope must reflect
        // it; this verifies the public behaviour end-to-end.
        _ = expectedStrokeCount;
        // Arrange
        var user = MakeUser("alice");
        var payload = new SignaturePadPayload
        {
            CoordinateMax = 10_000,
            CapturedAtUtc = new DateTime(2026, 7, 4, 12, 0, 0, DateTimeKind.Utc),
            Strokes = strokes,
        };

        // Act
        var fi = Run(user, 123L, SignatureType.Pro, payload);

        // Assert: file exists, sits under the user's root, and
        // parses as a yavsc.signature/v1 envelope.
        var fullPath = Path.Combine(fi.DestDir, fi.FileName);
        Assert.True(File.Exists(fullPath), $"missing: {fullPath}");

        using var doc = JsonDocument.Parse(File.ReadAllText(fullPath));
        var root = doc.RootElement;
        Assert.Equal("yavsc.signature/v1", root.GetProperty("format").GetString());
        Assert.Equal(10_000, root.GetProperty("coordinateMax").GetInt32());
        Assert.Equal(123L, root.GetProperty("estimateId").GetInt64());
        Assert.Equal("Pro", root.GetProperty("type").GetString());
        Assert.Equal("alice", root.GetProperty("signerName").GetString());
        Assert.Equal(expectedStrokeCount, root.GetProperty("strokeCount").GetInt32());
    }

    [Fact]
    public async Task ReceiveEstimateSignatureAsync_rejects_null_payload()
    {
        var user = MakeUser("bob");
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            EstimateSignatureFileHelper.ReceiveEstimateSignatureAsync(
                user, 1L, SignatureType.Pro, payload: null!));
    }

    [Fact]
    public async Task ReceiveEstimateSignatureAsync_rejects_non_positive_estimateId()
    {
        var user = MakeUser("bob");
        var payload = new SignaturePadPayload { Strokes = new[] { 1, 100, 100 } };
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            EstimateSignatureFileHelper.ReceiveEstimateSignatureAsync(
                user, 0L, SignatureType.Pro, payload));
    }

    // --- helpers ----------------------------------------------------

    private static FileReceivedInfo Run(
        ClaimsPrincipal user, long estimateId, SignatureType type, SignaturePadPayload payload)
    {
        // The helper is async; tests that don't care about the
        // result can call it sync via .GetAwaiter().GetResult()
        // because we know it never throws in the happy path.
        return EstimateSignatureFileHelper
            .ReceiveEstimateSignatureAsync(user, estimateId, type, payload, CancellationToken.None)
            .GetAwaiter().GetResult();
    }

    private static ClaimsPrincipal MakeUser(string username)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, username) },
            authenticationType: "test"));
    }
}
