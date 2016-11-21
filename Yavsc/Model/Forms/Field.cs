using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Model.Forms.Validation;

namespace Yavsc.Model.Forms
{
    public abstract class Field
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id {  get; set; }
        public abstract Method [] ValidationMethods ();

        public string Name { get; set; }

        public string Label { get; set; }
        public string PlaceHolder { get; set; }

        public string ValueType { get; set; }

    }
}