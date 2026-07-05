using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Models.Relationship;

namespace Yavsc.Models.Billing;

/// <summary>
/// One captured signature, attached to a single
/// <see cref="Estimate"/>. Multiple versions are allowed per
/// (EstimateId, Type, SignerId) tuple — the controller reads
/// the most recent when asked. The wire-format payload is the
/// same <c>int[]</c> shape PostIt produces (see
/// <c>PostIt.Models.SignaturePadData</c>): a length-prefixed
/// sequence of strokes, each stroke being
/// <c>[k, x0, y0, x1, y1, ...]</c> with <c>x, y ∈ [0,
/// CoordinateMax]</c>.
///
/// <para>
/// Why a separate table (instead of a JSON column on
/// <see cref="Estimate"/>): the jalon 1 spec calls for at least
/// two distinct signatures per estimate cycle (provider
/// validation + client agreement) and the audit value of
/// preserving superseded versions. A dedicated table also keeps
/// the <see cref="Estimate"/> row narrow, which matters for
/// list views.
/// </para>
/// </summary>
public class Signature
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long EstimateId { get; set; }

    [ForeignKey(nameof(EstimateId)), JsonIgnore]
    public virtual Estimate Estimate { get; set; }

    /// <summary>
    /// The <c>ApplicationUser.Id</c> of the signer. Always
    /// matches <c>Estimate.OwnerId</c> when
    /// <see cref="Type"/> is <see cref="SignatureType.Pro"/>, and
    /// <c>Estimate.ClientId</c> when
    /// <see cref="Type"/> is <see cref="SignatureType.Client"/>.
    /// The authz layer enforces this invariant; we don't
    /// duplicate the constraint in the schema to keep the model
    /// honest if a future business rule relaxes it (e.g. proxy
    /// signing).
    /// </summary>
    [Required]
    public string SignerId { get; set; }

    [ForeignKey(nameof(SignerId)), JsonIgnore]
    public virtual ApplicationUser Signer { get; set; }

    public SignatureType Type { get; set; }

    /// <summary>
    /// The normalised coordinate upper bound used at capture
    /// time. Today always
    /// <c>PostIt.Models.SignaturePadData.CoordinateMax</c>
    /// (10_000). Stored so a future change to the wire format
    /// can be replayed against old signatures without data
    /// loss.
    /// </summary>
    public int CoordinateMax { get; set; }

    /// <summary>
    /// Wire-format payload. PostgreSQL stores an <c>int[]</c>
    /// natively via Npgsql; the column is round-tripped through
    /// <c>JsonConvert</c> only if the migration binds it as
    /// <c>text</c> for backwards compatibility (see the EF
    /// configuration in <c>ApplicationDbContext</c>).
    /// </summary>
    [Required]
    public int[] Strokes { get; set; } = Array.Empty<int>();

    public DateTime CapturedAtUtc { get; set; }

    /// <summary>
    /// Path to the JSON-serialised wire payload on disk,
    /// relative to <c>UserFilesDirName</c>. The disk copy is the
    /// source of truth for the wire bytes; the <see cref="Strokes"/>
    /// column is a denormalised index for queries. They are
    /// written together in the same transaction by the
    /// controller; the migration should keep them in sync
    /// through <c>ApplicationDbContext.SaveChanges</c>.
    /// </summary>
    [Required]
    public string FilePath { get; set; }
}
