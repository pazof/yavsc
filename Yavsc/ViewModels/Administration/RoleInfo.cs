using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Yavsc.ViewModels.Administration
{
    public class RoleInfo
        {
            public RoleInfo ()
            {
                
            }
            public RoleInfo ( IdentityRole role)
            {
                Name = role.Name;
                  Id = role.Id;
                  Users = role.Users.Select(u => u.UserId).ToArray();
            }
            public string Id { get; set; }
            public string Name { get; set; }
            public string[] Users { get; set; }
        }
}