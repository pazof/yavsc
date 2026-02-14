using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Billing
{
    public class ExceptionSIREN {
        
        [Key, YaStringLength(9, 9)]
        public string SIREN { get; set; }
    }
}
