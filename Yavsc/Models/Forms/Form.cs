using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Forms
{

    public class Form
    {
        [Key]
        public string Id {get; set;}
        public string Summary { get; set; } 
    }
}