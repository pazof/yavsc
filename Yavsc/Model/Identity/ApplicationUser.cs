
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Identity;
using Yavsc.Model.Chat;

namespace Yavsc.Models
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Another me, as a byte array.
        /// This value points a picture that may be used 
        /// to sign documents.
        /// </summary>
        /// <returns>the path to an user's image, relative to it's dir<summary>
        /// <see>Startup.UserFilesOptions</see>
        /// </summary>
        /// <returns></returns>
        [MaxLength(512)]
        public string Avatar { get; set; }

        [MaxLength(512)]
        public string FullName { get; set; }

        
        /// <summary>
        /// WIP Paypal
        /// </summary>
        /// <returns></returns>
        [Display(Name="Account balance")]
        public virtual AccountBalance AccountBalance { get; set; }

        /// <summary>
        /// User's posts
        /// </summary>
        /// <returns></returns>
        [InverseProperty("Author")]
        public virtual List<Blog> Posts { get; set; }

        /// <summary>
        /// User's contact list
        /// </summary>
        /// <returns></returns>
        [InverseProperty("Owner")]
        public virtual List<Contact> Book { get; set; }

        /// <summary>
        /// External devices using the API
        /// </summary>
        /// <returns></returns>
        [InverseProperty("DeviceOwner")]
        public virtual List<GoogleCloudMobileDeclaration> Devices { get; set; }
        
        [InverseProperty("Owner")]
        public virtual List<Connection> Connections { get; set; }


        /// <summary>
        /// User's circles
        /// </summary>
        /// <returns></returns>
        [InverseProperty("Owner")]

        public virtual List<Circle> Circles { get; set; }

        /// <summary>
        /// Billing postal address
        /// </summary>
        /// <returns></returns>
        public virtual Location PostalAddress { get; set; }

        /// <summary>
        /// User's Google calendar
        /// </summary>
        /// <returns></returns>
        public string DedicatedGoogleCalendar { get; set; }

        public override string ToString() {
            return this.Id+" "+this.AccountBalance?.Credits.ToString()+this.Email+" "+this.UserName+" $"+this.AccountBalance?.Credits.ToString();
        }
    }
}
