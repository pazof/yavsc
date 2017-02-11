using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace Yavsc.Models.haircut
{
    public class HairTaint
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set;}

        public long Name { get; set; }

        public Color Color {get; set;}
    }
}