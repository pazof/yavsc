
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Yavsc.Models.Billing
{
    using Interfaces;
    using Models.Workflow;

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
        [ForeignKey("CommandId")]
        public BookQuery Query { get; set; }
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

        [Required]
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public virtual PerformerProfile Owner { get; set; }

        [Required]
        public string ClientId { get; set; }
        [ForeignKey("ClientId")]
        public virtual ApplicationUser Client { get; set; }

        public string CommandType
        {
            get; set;
        }
        public DateTime ProviderValidationDate { get; set; }
        public DateTime ClientValidationDate { get; set; }

    }
}
