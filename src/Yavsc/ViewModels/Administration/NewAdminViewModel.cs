using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels
{
    public partial class NewAdminViewModel {

        [Display(Name="NewAdminLabel", ResourceType=typeof(NewAdminViewModel))]
        [Required]
        public string NewAdminId { get; set; }
    }
}