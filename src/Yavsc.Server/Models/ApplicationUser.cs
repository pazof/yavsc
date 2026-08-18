using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using Yavsc.Models.Relationship;
using Yavsc.Models.Identity;
using Yavsc.Models.Chat;
using Yavsc.Models.Bank;
using Yavsc.Models.Access;
using Yavsc.Abstract.Identity;

namespace Yavsc.Models
{
    [Table("AspNetUsers")]
    public class ApplicationUser : IdentityUser, IApplicationUser
    {
        /// <summary> 
        /// Another me, as a byte array.TG7@Eu%80rufzkhbb
        /// This value points a picture that may be used
        /// to present the user
        /// </summary>
        /// <returns>the path to an user's image, relative to it's user dir<summary>
        /// <see>Startup.UserFilesOptions</see>
        /// </summary>
        /// <returns></returns>
        [MaxLength(512)]
        public string? Avatar { get; set; }

        [MaxLength(512)]
        public string? FullName { get; set; }


        /// <summary>
        /// WIP Paypal
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Account balance")]
        public virtual AccountBalance? AccountBalance { get; set; }

        /// <summary>
        /// User's posts
        /// </summary>
        /// <returns></returns>
        [InverseProperty("Author"), JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public virtual List<Blog.BlogPost>? Posts { get; set; }

        /// <summary>
        /// User's contact list
        /// </summary>
        /// <returns></returns>
        [InverseProperty("Owner"), JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public virtual List<Contact>? Book { get; set; }

        /// <summary>
        /// External devices using the API
        /// </summary>
        /// <returns></returns>
        [InverseProperty("DeviceOwner"), JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public virtual List<DeviceDeclaration>? DeviceDeclaration { get; set; }

        [InverseProperty("Owner"), JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public virtual List<ChatConnection>? Connections { get; set; }

        /// <summary>
        /// User's circles
        /// </summary>
        /// <returns></returns>
        [InverseProperty("Owner"), JsonIgnore, System.Text.Json.Serialization.JsonIgnore]

        public virtual List<Circle>? Circles { get; set; }

        /// <summary>
        /// Billing postal address
        /// </summary>
        /// <returns></returns>
        [ForeignKey("PostalAddressId")]
        public virtual Location? PostalAddress { get; set; }
        public long? PostalAddressId { get; set; }

        /// <summary>
        /// User's Google calendar
        /// </summary>
        /// <returns></returns>
        [MaxLength(512)]
        public string? DedicatedGoogleCalendar { get; set; }

        public override string ToString()
        {
            return this.Id + " " + this.AccountBalance?.Credits.ToString() + this.Email + " " + this.UserName + " $" + this.AccountBalance?.Credits.ToString();
        }

        public virtual List<BankIdentity>? BankInfo { get; set; }

        public long DiskQuota { get; set; } = 512 * 1024 * 1024;
        public long DiskUsage { get; set; } = 0;

        public long MaxFileSize { get; set; } = 512 * 1024 * 1024;

        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        [InverseProperty("Owner")]
        public virtual List<BlackListed>? BlackList { get; set; }

        public bool AllowMonthlyEmail { get; set; } = false;

        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        [InverseProperty("Owner")]
        public virtual List<ChatRoom>? Rooms { get; set; }

        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        [InverseProperty("User")]
        public virtual List<ChatRoomAccess>? RoomAccess { get; set; }

        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        [InverseProperty("Member")]
        public virtual List<CircleMember>? Membership { get; set; }

        /// <summary>
        /// User's blog comments
        /// </summary>
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        [InverseProperty("Author")]
        public virtual List<Blog.Comment>? BlogComments { get; set; }

        IAccountBalance? IApplicationUser.AccountBalance => AccountBalance;

        ILocation? IApplicationUser.PostalAddress { get => PostalAddress; }
    }
}
