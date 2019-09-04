using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels
{
    public partial class FireViewModel {
        
        [Display(Name="EnroledLabel", ResourceType=typeof(EnrolerViewModel))]
        public string EnroledUserName { get; set; }

        [YaRequired]
        public string EnroledUserId { get; set; }


        [Display(Name="RoleNameLabel", ResourceType=typeof(EnrolerViewModel))]
        [YaRequired]
        public string RoleName { get; set; }
    }
}