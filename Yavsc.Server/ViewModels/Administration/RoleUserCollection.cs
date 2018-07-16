using Yavsc.Abstract.Identity;

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