using Yavsc.Models.Billing;

namespace Yavsc.ViewModels.FrontOffice
{
    /// <summary>
    /// Body of <c>POST /api/v1/front/query/accept</c> and
    /// <c>.../reject</c>. The <see cref="Strokes"/> field is optional:
    /// when present (non-empty) on accept, it is the PostIt wire-format
    /// signature captured with the acceptance. All fields are optional
    /// so a caller may post <c>{}</c> to accept/reject without a
    /// signature.
    /// </summary>
    public class QueryAcceptanceRequest
    {
        /// <summary>
        /// Wire-format strokes. See
        /// <c>PostIt.Models.SignaturePadData</c>. Null or empty means
        /// no signature.
        /// </summary>
        public int[]? Strokes { get; set; }

        public int CoordinateMax { get; set; } = 10_000;

        public DateTime? CapturedAtUtc { get; set; }

        /// <summary>
        /// Which signature is being submitted:
        /// <see cref="SignatureType.Pro"/> (the provider) or
        /// <see cref="SignatureType.Client"/>. When <see cref="Strokes"/>
        /// is present this declares the side the caller is signing as;
        /// the server honors it only if the caller is that party
        /// (<c>query.PerformerId</c> for Pro, <c>query.ClientId</c>
        /// for Client). This disambiguates the case where one user is
        /// both parties, and makes "write only for its author"
        /// explicit. When null, the server falls back to the role
        /// inferred from the caller's identity.
        /// </summary>
        public SignatureType? SignatureType { get; set; }
    }
}
