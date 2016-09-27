using BookAStar.Model.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BookAStar.Model
{
    public class ClientProviderInfo
    {
        public string UserName { get; set; }
        
        public string Avatar { get; set; }
        // private string avatar;
        //  public string Avatar { get { return avatar ?? "icon-anon.png"; }  set { avatar = value; } }
        public string UserId { get; set; }
        public int Rate { get; set; }
        public string EMail { get; set; }
        public string Phone { get; set; }
        public Location BillingAddress { get; set; }
        public ImageSource AvatarOrNot
        {
            get
            {
                return Avatar ?? ImageSource.FromResource("BookAStar.icon-anon.png");
            }
        }
    }
}
