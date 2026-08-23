using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels
{
    public partial class EnrolerViewModel {

        [Required]
        public string EnroledUserId { get; set; }


        [Required]
        public string RoleName { get; set; }
    }
}
