using BookAStar.Helpers;
using BookAStar.Model.Social;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace BookAStar.Model
{
    public class ClientProviderInfo
    {
        public string UserName { get; set; }
        
        public string Avatar { get; set; }
        public string UserId { get; set; }
        public int Rate { get; set; }
        public string EMail { get; set; }
        public string Phone { get; set; }
        public Location BillingAddress { get; set; }
        // TODO Get User Professional status existence as a boolean
        // And hack the avatar with
        [JsonIgnore]
        public ImageSource AvatarOrNot
        {
            get
            {
                return UserHelpers.Avatar(Avatar);
            }
        }
    }
}
