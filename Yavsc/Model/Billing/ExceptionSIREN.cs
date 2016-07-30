using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Billing
{
    public class ExceptionSIREN {
        
        [Key]
        public string SIREN { get; set; }
    }
}