
using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Manage
{
    public class SetFullNameViewModel
    {
        [Required]
        [Display(Name = "Your full name"), YaStringLength(512)]
        public string FullName { get; set; }
    }
}
