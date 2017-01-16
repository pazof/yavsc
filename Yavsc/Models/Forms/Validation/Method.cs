using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Forms.Validation
{
    public class Method
    {
        [Key]
        public string Name {get; set; }
        
        /// <summary>
        /// TODO localisation ...
        /// </summary>
        /// <returns></returns>
        [Required]
        public string ErrorMessage { get; set; }
    }
}