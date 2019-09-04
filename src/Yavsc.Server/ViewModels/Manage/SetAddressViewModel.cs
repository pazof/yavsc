using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Manage
{
    public class SetAddressViewModel
    {
            [YaRequired]
            public string Street1 { get; set; }
            public string Street2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Country { get; set; }
            [YaRequired]
            public string PostalCode { get; set; }
    }
}
