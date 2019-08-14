using Microsoft.AspNet.Authorization;
using Yavsc.Services;
using Yavsc.ViewModels.Auth;

namespace Yavsc.AuthorizationHandlers {

    public class ViewFileHandler : AuthorizationHandler<ViewRequirement, ViewFileContext> {

        IFileSystemAuthManager _authManager;

        public ViewFileHandler (IFileSystemAuthManager authManager) {
            _authManager = authManager;
        }

        protected override void Handle (AuthorizationContext context, ViewRequirement requirement, ViewFileContext fileContext) {
            // TODO file access rules
            if (fileContext.Path.StartsWith ("/pub/"))
                context.Succeed (requirement);
            else {
                if (!fileContext.Path.StartsWith ("/"))
                    context.Fail ();
                else {
                    var rights = _authManager.GetFilePathAccess (context.User, fileContext.Path);
                    if ((rights & FileAccessRight.Read) > 0)
                        context.Succeed (requirement);
                    else context.Fail ();
                }
            }
        }
    }
}