using ZicMoove.Helpers;
using ZicMoove.Model.Social;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ZicMoove.Model
{
    public class ClientProviderInfo
    {
        public string UserName { get; set; }
        
        public string Avatar { get; set; }
        
        public string UserId { get; set; }
        // TODO Get User Professional status existence as a boolean
        // And hack the avatar with
        public int Rate { get; set; }

        public string EMail { get; set; }
        public string Phone { get; set; }
        public Location BillingAddress { get; set; }
       

        public override string ToString()
        {
            return UserName;
        }
    }
}
