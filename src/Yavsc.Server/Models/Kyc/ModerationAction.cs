// Yavsc.Models.Kyc/ModerationLog.cs
namespace Yavsc.Models.Kyc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public enum ModerationAction { Approved, Rejected, Redacted }

    /// <summary>
    /// Trace immuable de chaque décision de modération.
    /// Pseudonymisée : ModeratorId est l'Id ASP.NET Identity, pas un nom.
    /// </summary>
    public class ModerationLog
    {
        [Key]
        public long Id { get; set; }

        public long DeclarationId { get; set; }
        public virtual TrustDeclaration Declaration { get; set; }

        public string ModeratorId { get; set; }  // ASP.NET Identity UserId
        public ModerationAction Action { get; set; }
        public int ScoreDelta { get; set; }
        public DateTime Timestamp { get; set; }
    }
}