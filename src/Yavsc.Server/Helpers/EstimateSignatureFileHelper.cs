using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Server.Models.FileSystem;
namespace Yavsc.Server.Helpers;

/// <summary>
/// Filesystem counterpart of <see cref="Signature"/>: writes
/// the wire-format JSON payload next to the user's other files,
/// under <c>signatures/</c>, and updates the user's disk quota.
///
/// This is the JSON counterpart of the legacy
/// <c>ReceiveProSignatureAsync</c> method, which stored
/// <c>sign-{billingCode}-{signType}-{estimateId}.png</c> blobs.
/// We don't reuse that helper because (a) the wire format is no
/// longer a binary image, (b) there's no <c>billingCode</c> on
/// a freshly signed estimate in our model, and (c) the legacy
/// helper takes an <see cref="IFormFile"/> whereas our pipeline
/// decodes a JSON body upstream of the controller and passes
/// <see cref="SignaturePadPayload"/> in directly.
/// </summary>
public static class EstimateSignatureFileHelper
{
    /// <summary>
    /// Sub-directory under the user's root where signature
    /// payloads live. Kept short to leave room in PATH_MAX on
    /// legacy filesystems; the rest of the filename is
    /// <c>sign-{type}-{estimateId}-{utcTicks}.json</c>.
    /// </summary>
    public const string SignaturesSubdir = "signatures";

    /// <summary>
    /// Format string for signature file names. Public so the
    /// migration and the admin tools can list by pattern.
    /// </summary>
    public static string FileNameFormat(SignatureType type, long estimateId, long utcTicks)
        => $"sign-{type.ToString().ToLowerInvariant()}-{estimateId}-{utcTicks}.json";

    /// <summary>
    /// Persist a signature wire payload to disk. Returns the
    /// file info (relative path under the user's root) suitable
    /// for storing in <see cref="Signature.FilePath"/>; the
    /// caller is responsible for the database write.
    /// </summary>
    /// <param name="user">Signed-in user. Their
    /// <c>Identity.Name</c> locates the disk root via
    /// <see cref="AbstractFileSystemHelpers.UserFilesDirName"/>.
    /// </param>
    /// <param name="estimateId">Estimate this signature
    /// attaches to. Used in the file name for human inspection
    /// and to support multiple versions over time.</param>
    /// <param name="type">Provider or client signature.</param>
    /// <param name="payload">Decoded wire payload (strokes +
    /// coordinateMax + capturedAtUtc). Already validated
    /// upstream.</param>
    /// <param name="token">Cancellation token forwarded to
    /// the file write.</param>
    public static async Task<FileReceivedInfo> ReceiveEstimateSignatureAsync(
        this ClaimsPrincipal user,
        long estimateId,
        SignatureType type,
        SignaturePadPayload payload,
        CancellationToken token = default)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        if (payload is null) throw new ArgumentNullException(nameof(payload));
        if (estimateId <= 0) throw new ArgumentOutOfRangeException(nameof(estimateId));

        // Ensure the user has a /signatures/ sub-directory we can
        // write to. EnsureDestinationDirectory throws on invalid
        // paths and creates the directory on the way; the
        // SignaturesSubdir constant is a server-controlled value
        // (not user-derived), so we skip the IsValidYavscPath
        // check that ReceiveUserFile performs on user-supplied
        // subpaths.
        var root = user.EnsureDestinationDirectory(SignaturesSubdir);

        var fileName = FileNameFormat(type, estimateId, DateTime.UtcNow.Ticks);
        var fullPath = Path.Combine(root, fileName);

        var envelope = new
        {
            format = "yavsc.signature/v1",
            coordinateMax = payload.CoordinateMax,
            capturedAtUtc = payload.CapturedAtUtc,
            estimateId,
            type = type.ToString(),
            // Identity.Name is the username; we keep the wire
            // payload keyed on the username rather than the
            // numeric/guid Id so disk-side human inspection
            // (e.g. cat sign-pro-1234-...json) is self-evident.
            signerName = user.Identity?.Name,
            strokes = payload.Strokes,
            strokeCount = CountStrokes(payload.Strokes),
        };

        var json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(fullPath, json, Encoding.UTF8, token).ConfigureAwait(false);

        // Quota update is the controller's responsibility: the
        // helper has no DbContext access, and a ClaimsPrincipal
        // is not an ApplicationUser. The controller looks up
        // the user by Identity.Name and bumps DiskUsage after
        // a successful database write.

        return new FileReceivedInfo(root, fileName);
    }

    private static int CountStrokes(int[] strokes)
    {
        int n = 0;
        for (int i = 0; i < strokes.Length;)
        {
            int k = strokes[i];
            if (k <= 0) break;
            n++;
            i += 1 + 2 * k;
        }
        return n;
    }
}

/// <summary>
/// Wire payload accepted by the signature endpoint and
/// persisted by <see cref="EstimateSignatureFileHelper"/>.
/// Mirrors <c>PostIt.Models.SignaturePadData</c>'s JSON shape
/// (without the disk-only envelope fields) so the two sides
/// stay trivially compatible.
/// </summary>
public class SignaturePadPayload
{
    /// <summary>
    /// Normalised coordinate upper bound. Must be
    /// <c>PostIt.Models.SignaturePadData.CoordinateMax</c>
    /// (10_000) today; declared as a property so a future
    /// resolution change can be replayed against the same
    /// wire format.
    /// </summary>
    public int CoordinateMax { get; set; } = 10_000;

    /// <summary>
    /// Client-reported capture time. The server may ignore
    /// this for ordering (UTC now is the truth) but keeps it
    /// for round-trip display.
    /// </summary>
    public DateTime CapturedAtUtc { get; set; }

    /// <summary>
    /// Wire strokes. See
    /// <c>PostIt.Models.SignaturePadData</c> for the format.
    /// </summary>
    public int[] Strokes { get; set; } = Array.Empty<int>();
}
