using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels.Manage
{
    public class SetAddressViewModel
    {
            [Required]
            public string Street1 { get; set; }
            public string Street2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Country { get; set; }
            [Required]
            public string PostalCode { get; set; }
    }
}
