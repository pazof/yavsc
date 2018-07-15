using System.Linq;

namespace Yavsc.ViewModels.Administration
{
    public class RoleInfo
        {
            public RoleInfo ()
            {
                
            }
            public RoleInfo ( string roleName, string roleId, string[] users)
            {
                Name = roleName; // role.Name;
                  Id = roleId; // role.Id;
                  Users = users ; // role.Users.Select(u => u.UserId).ToArray();
            }
            public string Id { get; set; }
            public string Name { get; set; }
            public string[] Users { get; set; }
        }
}