
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{
    public partial class Projet
    {
        [Key(),DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKeyAttribute("AspNetUsers.Id")]
        public string ManagerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
