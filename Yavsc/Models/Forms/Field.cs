using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Forms
{
    using Validation;
    public abstract class Field
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id {  get; set; }
        public abstract Method [] ValidationMethods ();

        [Required]
        public string Name { get; set; }

        public string Label { get; set; }
        
        public string PlaceHolder { get; set; }
        
        [Required]
        public string ValueType { get; set; }

    }
}