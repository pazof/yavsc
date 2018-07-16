using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Identity;
using Yavsc.Models.Auth;

namespace Yavsc.ViewModels.Administration
{
    public class RoleUserCollection
    {
        public RoleUserCollection()
        {
            
        }
         public string Id { get; set; }
            public string Name { get; set; }
            public UserInfo[] Users { get; set; }
    }
}