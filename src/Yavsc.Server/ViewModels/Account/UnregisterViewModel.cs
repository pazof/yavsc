using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Account
{
    public class UnregisterViewModel
    {
        [YaRequired]
        public string UserId { get; set; }
        
        public string ReturnUrl { get; set; }

    }
}
