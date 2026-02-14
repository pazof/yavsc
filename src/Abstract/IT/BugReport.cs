using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ApiControllers
{
    public class BugReport {
        [Required,YaStringLength(1024)]
        public string ApiKey { get ; set; }
        [Required,YaStringLength(512)]
        public string Component { get ; set; }
        [Required][YaStringLength(10240)]
        public string ExceptionObjectJson { get ; set; }
    }
}

