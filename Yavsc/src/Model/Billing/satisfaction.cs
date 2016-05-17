
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{
    public partial class satisfaction
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long _id { get; set; }
        public string comnt { get; set; }
        public int? rate { get; set; }

        [ForeignKey("Skill.Id")]
        public long? userskillid { get; set; }
    }
}
