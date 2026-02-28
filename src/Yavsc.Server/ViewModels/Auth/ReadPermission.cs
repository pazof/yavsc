using Microsoft.AspNetCore.Authorization;

namespace Yavsc.ViewModels.Auth
{
    public class ReadPermission: IAuthorizationRequirement
    {
        public ReadPermission()
        {
        }
    }
    
}
