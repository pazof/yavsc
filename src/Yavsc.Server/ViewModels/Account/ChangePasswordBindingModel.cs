
using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Account { 
    public class ChangePasswordBindingModel {
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
    public class SetPasswordBindingModel {
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

    }
}
