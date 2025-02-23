using Microsoft.AspNetCore.Authorization;

namespace Yavsc.ViewModels.Auth
{
    public class DeletePermission: IAuthorizationRequirement
    {
        public DeletePermission()
        {
        }
    }
    
}
