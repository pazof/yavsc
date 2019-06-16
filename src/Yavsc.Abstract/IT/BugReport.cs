using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ApiControllers
{
    public class BugReport {
        [Required]
        public string ApiKey { get ; set; }
        [Required]
        public string Component { get ; set; }
        [Required][YaStringLength(1024)]
        public string ExceptionObjectJson { get ; set; }
    }
}

