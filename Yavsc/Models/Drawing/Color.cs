using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Drawing
{
    public class Color
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public Color(){

        }
        public Color(Color c)
        {
            Red=c.Red;
            Green=c.Green;
            Blue=c.Blue;
            Name=c.Name;
        }
        public byte Red {get;set;}
        public byte Green {get;set;}
        public byte Blue {get;set;}
        public string Name { get; set; }
    }
}