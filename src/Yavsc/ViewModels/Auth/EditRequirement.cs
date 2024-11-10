using Microsoft.AspNetCore.Authorization;

namespace Yavsc.ViewModels.Auth
{
    public class EditPermission : IAuthorizationRequirement
    {
        public EditPermission()
        {
        }
    }

    public class ReadPermission: IAuthorizationRequirement
    {
        public ReadPermission()
        {
        }
    }

    public class DeletePermission: IAuthorizationRequirement
    {
        public DeletePermission()
        {
        }
    }
    
}
