#nullable enable annotations

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Yavsc.Models.Billing
{
    using Models.Workflow;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class Estimate : IEstimate
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? CommandId { get; set; }
        /// <summary>
        /// A command is not required to create
        /// an estimate,
        /// it will result in a new estimate template
        /// </summary>
        /// <returns></returns>
        [ForeignKey("CommandId"),JsonIgnore,ValidateNever]
        public NominativeServiceCommand? Query { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }

        [InverseProperty("Estimate")]
        public virtual List<CommandLine> Bill { get; set; }
        /// <summary>
        /// List of attached graphic files
        /// to this estimate, as relative pathes to
        /// the command performer's root path.
        /// In db, they are separated by <c>:</c>
        /// </summary>
        /// <returns></returns>
        [NotMapped]
        public List<string> AttachedGraphics { get; set; }

        public string AttachedGraphicsString
        {
            get { return string.Join(":", AttachedGraphics); }
            set { AttachedGraphics = value.Split(':').ToList(); }
        }
        /// <summary>
        /// List of attached files
        /// to this estimate, as relative pathes to
        /// the command performer's root path.
        /// In db, they are separated by <c>:</c>
        /// </summary>
        /// <returns></returns>
        [NotMapped]
        public List<string> AttachedFiles { get; set; }
        public string AttachedFilesString
        {
            get { return string.Join(":", AttachedFiles); }
            set { AttachedFiles = value.Split(':').ToList(); }
        }

        [ValidateNever]
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId"),JsonIgnore,ValidateNever]
        public virtual PerformerProfile Owner { get; set; }

        [Required]
        public string ClientId { get; set; }
        [ForeignKey("ClientId"),JsonIgnore,ValidateNever]
        public virtual ApplicationUser Client { get; set; }

        [Required]
        public string CommandType
        {
            get; set;
        }
        public DateTime ProviderValidationDate { get; set; }
        public DateTime ClientValidationDate { get; set; }

        /// <summary>
        /// All signatures captured against this estimate, keyed by
        /// <see cref="SignatureType"/>. The persister upserts by
        /// (EstimateId, Type), so there is at most one row per type.
        /// This collection is the persisted shape and stays
        /// <c>[JsonIgnore]</c>: the wire payload exposes signatures
        /// only through the dedicated <see cref="SignaturePro"/> and
        /// <see cref="SignatureClient"/> projections, since an
        /// estimate has exactly these two signature slots and never
        /// more. See <see cref="Signature"/>.
        /// </summary>
        [InverseProperty(nameof(Signature.Estimate))]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Signature> Signatures { get; set; }
            = new List<Signature>();

        /// <summary>
        /// The provider's signature on this estimate, or null when the
        /// provider has not signed. Projected from
        /// <see cref="Signatures"/>; not mapped (the table is
        /// <see cref="Signatures"/>).
        /// </summary>
        [NotMapped]
        public Signature? SignaturePro
            => Signatures?.FirstOrDefault(s => s.Type == SignatureType.Pro);

        /// <summary>
        /// The client's signature on this estimate, or null when the
        /// client has not signed. Projected from
        /// <see cref="Signatures"/>; not mapped.
        /// </summary>
        [NotMapped]
        public Signature? SignatureClient
            => Signatures?.FirstOrDefault(s => s.Type == SignatureType.Client);

    }
}
