
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Identity;

namespace Yavsc.Models
{

    public class ApplicationUser : IdentityUser, IApplicationUser
    {

        [Display(Name="AccountBalance")]
        public virtual IAccountBalance AccountBalance { get; set; }

        [InverseProperty("Author")]
        public virtual IList<IBlog> Posts { get; set; }

        [InverseProperty("Owner")]
        public virtual IList<IContact> Book { get; set; }

        [InverseProperty("DeviceOwner")]
        public virtual IList<IGoogleCloudMobileDeclaration> Devices { get; set; }
        
        [InverseProperty("Owner")]

        public virtual IList<ICircle> Circles { get; set; }
        public virtual ILocation PostalAddress { get; set; }

        public string DedicatedGoogleCalendar { get; set; }

        public override string ToString() {
            return this.Id+" "+this.AccountBalance?.Credits.ToString()+this.Email+" "+this.UserName+" $"+this.AccountBalance?.Credits.ToString();
        }
    }
}
