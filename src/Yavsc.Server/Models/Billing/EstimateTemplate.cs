using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Billing
{
    public partial class EstimateTemplate
    {

        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public List<CommandLine> Bill { get; set; }
        
        [Required]
        public string OwnerId { get; set; }
    }

}