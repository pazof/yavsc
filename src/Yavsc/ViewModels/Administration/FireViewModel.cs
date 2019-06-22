using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels
{
    public partial class FireViewModel {
        
        [Display(Name="EnroledLabel", ResourceType=typeof(EnrolerViewModel))]
        public string EnroledUserName { get; set; }

        [Required]
        public string EnroledUserId { get; set; }


        [Display(Name="RoleNameLabel", ResourceType=typeof(EnrolerViewModel))]
        [Required]
        public string RoleName { get; set; }
    }
}