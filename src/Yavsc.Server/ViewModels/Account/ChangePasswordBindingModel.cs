
using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Account { 
    public class ChangePasswordBindingModel {
        [YaRequired]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [YaRequired]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
    public class SetPasswordBindingModel {
        [YaRequired]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

    }
}