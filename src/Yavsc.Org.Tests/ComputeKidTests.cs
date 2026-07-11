using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Xunit;
using Yavsc.Extensions;

namespace Yavsc.Org.Tests;

/// <summary>
/// Tests for <see cref="HostingExtensions.ComputeKid"/>, the
/// helper that derives the JWT <c>kid</c> header / JWKS key id
/// from the signing certificate. The kid is consumed by every
/// resource server (Yavsc.Blogs, Yavsc.Api) to match a token to
/// the right key in the JWKS, so getting its shape and stability
/// right is the whole point of the fix in commit 2c6d1157
/// (IDX10500 regression).
/// </summary>
/// <remarks>
/// We don't load the production cert (Let's Encrypt PEM + RSA
/// private key) — we generate throwaway self-signed certs in a
/// temp dir. The contract under test is the truncation /
/// encoding of the thumbprint, which is independent of the key
/// type and the cert issuer.
/// </remarks>
public class ComputeKidTests : IDisposable
{
    private readonly string _tempDir;

    public ComputeKidTests()
    {
        _tempDir = Path.Combine(
            Path.GetTempPath(),
            "yavsc-compute-kid-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch { /* best effort — Temp gets cleaned eventually */ }
    }

    [Fact]
    public void ComputeKid_returns_first_16_hex_chars_of_cert_thumbprint()
    {
        var certPath = WriteSelfSignedCertRsa(out var expectedThumbHex);

        var kid = HostingExtensions.ComputeKid(certPath);

        // 16 hex chars = 64 bits, enough to be globally unique
        // within a deployment and compact enough for a JWT header.
        Assert.Equal(16, kid.Length);
        Assert.True(
            kid.All(c => "0123456789ABCDEF".Contains(c)),
            $"kid '{kid}' contains non-uppercase-hex characters");

        // Match the first 16 chars of the thumbprint exactly. We
        // compute the expected value from the same cert the helper
        // was given — no magic constants, no copy-paste of the
        // truncation logic under test.
        Assert.Equal(expectedThumbHex[..16], kid);
    }

    [Fact]
    public void ComputeKid_is_stable_across_repeated_reads()
    {
        var certPath = WriteSelfSignedCertRsa(out _);

        var first = HostingExtensions.ComputeKid(certPath);
        var second = HostingExtensions.ComputeKid(certPath);
        var third = HostingExtensions.ComputeKid(certPath);

        // Stability matters: a non-deterministic kid would
        // invalidate tokens on every IdentityServer restart.
        Assert.Equal(first, second);
        Assert.Equal(second, third);
    }

    [Fact]
    public void ComputeKid_differs_between_distinct_certificates()
    {
        var certPathA = WriteSelfSignedCertRsa(out _);
        var certPathB = WriteSelfSignedCertRsa(out _);

        var kidA = HostingExtensions.ComputeKid(certPathA);
        var kidB = HostingExtensions.ComputeKid(certPathB);

        // Two independent RNG-drawn RSA keys will (in practice
        // always) yield different thumbprints. A 64-bit truncated
        // space has collisions at ~2^32 certs; we won't get there.
        Assert.NotEqual(kidA, kidB);
    }

    [Fact]
    public void ComputeKid_uses_thumbprint_not_subject_or_serial()
    {
        // The previous fix-message claimed SHA-256; the helper
        // actually reads X509Certificate2.GetCertHash() which is
        // SHA-1. Pin that behaviour so a future refactor that
        // switches to SHA-256 (or any other digest) is forced to
        // update the test deliberately.
        var certPath = WriteSelfSignedCertRsa(out var thumbHex);

        var kid = HostingExtensions.ComputeKid(certPath);

        // 16 hex chars is half of a 20-byte SHA-1 thumbprint.
        // SHA-256 would be 32 bytes (64 hex chars) before
        // truncation; SHA-1 is the only common digest whose
        // hex encoding fits the 16-char prefix we observe.
        Assert.Equal(20, thumbHex.Length / 2);
        Assert.Equal(thumbHex[..16], kid);
    }

    [Fact]
    public void ComputeKid_propagates_cryptographic_exception_for_missing_file()
    {
        // The wrapper LoadSigningCredentials wraps this in an
        // InvalidOperationException, but ComputeKid itself is a
        // plain helper — it must surface the parser error so the
        // wrapper can attach the cert path to the message. We
        // assert against the base CryptographicException rather
        // than the concrete subtype because the runtime picks
        // different leaf types per platform (on Linux/OpenSSL we
        // get Interop+Crypto+OpenSslCryptographicException, on
        // Windows we'd get the older CryptographicException
        // directly); the contract is the same either way.
        var missing = Path.Combine(_tempDir, "does-not-exist.pem");

        Assert.ThrowsAny<CryptographicException>(
            () => HostingExtensions.ComputeKid(missing));
    }

    // --- helpers ----------------------------------------------------

    /// <summary>
    /// Generate a throwaway self-signed RSA-2048 cert, export it
    /// as PEM to a file inside the test temp dir, and return the
    /// path. The out parameter receives the upper-case hex form
    /// of the cert's SHA-1 thumbprint so tests can pin the
    /// expected kid without re-implementing the helper.
    /// </summary>
    private string WriteSelfSignedCertRsa(out string thumbHex)
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            "CN=yavsc-test",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        using var cert = req.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(1));

        // Capture the thumbprint before exporting — the cert is
        // disposed by `using` and the exported PEM is what the
        // helper will read.
        thumbHex = Convert.ToHexString(cert.GetCertHash());

        var path = Path.Combine(_tempDir, "cert-" + Guid.NewGuid().ToString("N") + ".pem");
        File.WriteAllText(path, cert.ExportCertificatePem());
        return path;
    }
}
