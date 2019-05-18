using System.ComponentModel.DataAnnotations;

namespace Yavsc.ApiControllers
{
    public class BugReport {
        [Required]
        public string ApiKey { get ; set; }
        [Required]
        public string Component { get ; set; }
        [Required][StringLength(1024)]
        public string ExceptionObjectJson { get ; set; }
    }
}

