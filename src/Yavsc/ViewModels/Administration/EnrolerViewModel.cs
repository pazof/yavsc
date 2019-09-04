using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels
{
    public partial class EnrolerViewModel {

        [Display(Name="EnroledLabel", ResourceType=typeof(EnrolerViewModel))]
        [YaRequired]
        public string EnroledUserId { get; set; }


        [Display(Name="RoleNameLabel", ResourceType=typeof(EnrolerViewModel))]
        [YaRequired]
        public string RoleName { get; set; }
    }
}