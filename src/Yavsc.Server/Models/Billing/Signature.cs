using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Billing;

/// <summary>
/// One captured signature, attached to a single
/// <see cref="Estimate"/>. Multiple versions are allowed per
/// (EstimateId, Type, SignerId) tuple — the controller reads the
/// most recent when asked. The wire-format payload is the
/// same <c>int[]</c> shape PostIt produces (see
/// <c>PostIt.Models.SignaturePadData</c>): a length-prefixed
/// sequence of strokes, each stroke being
/// <c>[k, x0, y0, x1, y1, ...]</c> with <c>x, y ∈ [0,
/// CoordinateMax]</c>.
///
/// <para>
/// The <see cref="Strokes"/> column is the source of truth: the
/// signature is captured and stored entirely in-app (PostIt) and
/// round-tripped as JSON, with no on-disk copy. A dedicated table
/// (instead of a JSON column on <see cref="Estimate"/>) keeps the
/// <see cref="Estimate"/> row narrow for list views and preserves
/// superseded versions for audit.
/// </para>
/// </summary>
public class Signature
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long EstimateId { get; set; }

    [ForeignKey(nameof(EstimateId)), JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
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

    [ForeignKey(nameof(SignerId)), JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
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
}