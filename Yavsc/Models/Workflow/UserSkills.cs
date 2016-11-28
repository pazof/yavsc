
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{
    public partial class UserSkills
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [ForeignKeyAttribute("AspNetUsers.Id")]
        public string UserId { get; set; }

        [ForeignKeyAttribute("Skill.Id")]
        public long SkillId { get; set; }

        public string Comment { get; set; }
        public int Rate { get; set; }
    }
}
