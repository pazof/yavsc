
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Identity;

namespace Yavsc.Models
{

    public class ApplicationUser : IdentityUser
    {

        [Display(Name="AccountBalance")]
        public virtual AccountBalance AccountBalance { get; set; }

        [InverseProperty("Author")]
        public virtual List<Blog> Posts { get; set; }

        [InverseProperty("Owner")]
        public virtual List<Contact> Book { get; set; }

        [InverseProperty("DeviceOwner")]
        public virtual List<GoogleCloudMobileDeclaration> Devices { get; set; }
        
        [InverseProperty("Owner")]

        public virtual List<Circle> Circles { get; set; }
        public virtual Location PostalAddress { get; set; }

        public string DedicatedGoogleCalendar { get; set; }

        public override string ToString() {
            return this.Id+" "+this.AccountBalance?.Credits.ToString()+this.Email+" "+this.UserName+" $"+this.AccountBalance?.Credits.ToString();
        }
    }
}
