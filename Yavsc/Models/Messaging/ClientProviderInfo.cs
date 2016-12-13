
using System.ComponentModel.DataAnnotations;
namespace Yavsc.Models.Messaging
{
    public class ClientProviderInfo
    {
        public string UserName { get; set; }
        public string Avatar { get; set; }
        [Key]
        public string UserId { get; set; }
        public string EMail { get; set; }
        public string Phone { get; set; }
        public Location BillingAddress { get; set; }
    }
}
