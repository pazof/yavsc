#nullable enable annotations

using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Billing;

namespace Yavsc.Server.Helpers;

/// <summary>
/// Shared signature-persistence logic for estimates, extracted from
/// <c>BillingController.Sign</c>. Stages the <see cref="Signature"/>
/// row (find-or-add by <c>(EstimateId, Type)</c>) for the PostIt
/// wire-format payload — but does <b>not</b> call
/// <c>SaveChanges</c>. The caller owns the transaction so it can
/// combine the signature write with its own changes (e.g. setting a
/// validation date or a query status) in a single
/// <c>SaveChanges</c>.
///
/// <para>
/// The <see cref="Signature.Strokes"/> column is the source of
/// truth; there is no on-disk copy, so no disk-quota accounting is
/// involved.
/// </para>
/// </summary>
public static class EstimateSignaturePersister
{
    /// <summary>
    /// Stage a signature for <paramref name="estimateId"/> signed by
    /// <paramref name="signerId"/> as <paramref name="type"/>. Upserts
    /// the <see cref="Signature"/> row for the
    /// <c>(EstimateId, Type)</c> pair, writing the wire payload
    /// (<c>Strokes</c>, <c>CoordinateMax</c>, <c>CapturedAtUtc</c>)
    /// and <paramref name="signerId"/>. Returns the staged
    /// <see cref="Signature"/>; the caller persists with
    /// <c>SaveChanges</c>.
    /// </summary>
    public static async Task<Signature> StageAsync(
        ApplicationDbContext db,
        long estimateId,
        SignatureType type,
        string signerId,
        SignaturePadPayload payload,
        CancellationToken token = default)
    {
        if (db is null) throw new ArgumentNullException(nameof(db));
        if (payload is null) throw new ArgumentNullException(nameof(payload));
        if (estimateId <= 0) throw new ArgumentOutOfRangeException(nameof(estimateId));

        // Find-or-add: the (EstimateId, Type) pair is unique, so a
        // second submission for the same side of the estimate
        // replaces the previous signature. EF translates this into a
        // single UPDATE when the row exists and an INSERT otherwise;
        // the unique index in ApplicationDbContext is the
        // database-level guarantee that the contract holds if two
        // requests race.
        var signature = await db.Signatures
            .FirstOrDefaultAsync(s => s.EstimateId == estimateId && s.Type == type, token)
            .ConfigureAwait(false);

        if (signature is null)
        {
            signature = new Signature
            {
                EstimateId = estimateId,
                SignerId = signerId,
                Type = type,
            };
            db.Signatures.Add(signature);
        }

        signature.SignerId = signerId;
        signature.CoordinateMax = payload.CoordinateMax;
        signature.Strokes = payload.Strokes;
        signature.CapturedAtUtc = payload.CapturedAtUtc;

        return signature;
    }
}

/// <summary>
/// Wire payload accepted by the signature path and staged by
/// <see cref="EstimateSignaturePersister"/>. Mirrors
/// <c>PostIt.Models.SignaturePadData</c>'s JSON shape (without the
/// disk-only envelope fields) so the two sides stay trivially
/// compatible.
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