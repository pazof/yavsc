using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Yavsc.Models.Haircut
{
    using Drawing;
    public class HairTaint
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set;}

        public string Brand { get; set; }

        [Required]
        public long ColorId { get; set; }

        [ForeignKeyAttribute("ColorId")]
        public virtual Color Color {get; set;}
    }
}