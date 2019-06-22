using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels
{
    public partial class EnrolerViewModel {

        [Display(Name="EnroledLabel", ResourceType=typeof(EnrolerViewModel))]
        [Required]
        public string EnroledUserId { get; set; }


        [Display(Name="RoleNameLabel", ResourceType=typeof(EnrolerViewModel))]
        [Required]
        public string RoleName { get; set; }
    }
}