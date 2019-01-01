
using System.ComponentModel.DataAnnotations;
namespace Yavsc.Abstract.Identity
{
    public class ClientProviderInfo
    {
        public string UserName { get; set; }
        public string Avatar { get; set; }

        [Key]
        public string UserId { get; set; }
        public string EMail { get; set; }
        public string Phone { get; set; }
        public long BillingAddressId { get; set; }
    }
}
