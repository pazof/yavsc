using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Billing
{
    public class ExceptionSIREN {
        
        [Key,MinLength(9)]
        public string SIREN { get; set; }
    }
}