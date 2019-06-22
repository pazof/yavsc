
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{
    using Models.Relationship;
    using Models.Identity;
    using Models.Chat;
    using Models.Bank;
    using Models.Access;
    using Newtonsoft.Json;
    using System;

    public class ApplicationUser : IdentityUser, IBaseTrackedEntity
    {
        /// <summary>
        /// Another me, as a byte array.
        /// This value points a picture that may be used
        /// to present the user
        /// </summary>
        /// <returns>the path to an user's image, relative to it's user dir<summary>
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
        [InverseProperty("Author"),JsonIgnore]
        public virtual List<Blog.BlogPost> Posts { get; set; }

        /// <summary>
        /// User's contact list
        /// </summary>
        /// <returns></returns>
        [InverseProperty("Owner"),JsonIgnore]
        public virtual List<Contact> Book { get; set; }

        /// <summary>
        /// External devices using the API
        /// </summary>
        /// <returns></returns>
        [InverseProperty("DeviceOwner"),JsonIgnore]
        public virtual List<DeviceDeclaration> DeviceDeclaration { get; set; }

        [InverseProperty("Owner"),JsonIgnore]
        public virtual List<ChatConnection> Connections { get; set; }


        /// <summary>
        /// User's circles
        /// </summary>
        /// <returns></returns>
        [InverseProperty("Owner"),JsonIgnore]

        public virtual List<Circle> Circles { get; set; }

        /// <summary>
        /// Billing postal address
        /// </summary>
        /// <returns></returns>
        [ForeignKeyAttribute("PostalAddressId")]
        public virtual Location PostalAddress { get; set; }
        public long? PostalAddressId { get; set; }

        /// <summary>
        /// User's Google calendar
        /// </summary>
        /// <returns></returns>
        [MaxLength(512)]
        public string DedicatedGoogleCalendar { get; set; }

        public override string ToString() {
            return this.Id+" "+this.AccountBalance?.Credits.ToString()+this.Email+" "+this.UserName+" $"+this.AccountBalance?.Credits.ToString();
        }

        public BankIdentity BankInfo { get; set; }

        public long DiskQuota { get; set; } = 512*1024*1024;
        public long DiskUsage { get; set; } = 0;

        public long MaxFileSize { get; set; } = 512*1024*1024;

        [JsonIgnore][InverseProperty("Owner")]
        public virtual List<BlackListed> BlackList { get; set; }

        public bool AllowMonthlyEmail { get; set; } = false;

        [JsonIgnore][InverseProperty("Owner")]
        public virtual List<ChatRoom> Rooms { get; set; }

        [JsonIgnore][InverseProperty("User")]
        public virtual List<ChatRoomAccess> RoomAccess { get; set; }

        public DateTime DateCreated
        {
            get; set;
        }

        public string UserCreated
        {
            get; set;
        }        
        
        public DateTime DateModified 
        {
            get; set;
        }        
        
        public string UserModified
        {
            get; set;
        }
    }
}
