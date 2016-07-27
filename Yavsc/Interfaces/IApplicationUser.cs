using System.Collections.Generic;
using Yavsc.Models.Identity;

namespace Yavsc.Models
{
    public interface IApplicationUser
    {
        IAccountBalance AccountBalance { get; set; }
        IList<IContact> Book { get; set; }
        IList<ICircle> Circles { get; set; }
        string DedicatedGoogleCalendar { get; set; }
        IList<IGoogleCloudMobileDeclaration> Devices { get; set; }
        ILocation PostalAddress { get; set; }
        IList<IBlog> Posts { get; set; }
    }
}
