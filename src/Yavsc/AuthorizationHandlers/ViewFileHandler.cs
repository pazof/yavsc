using Microsoft.AspNet.Authorization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Yavsc.Services;
using Yavsc.ViewModels.Auth;

namespace Yavsc.AuthorizationHandlers
{

    public class ViewFileHandler : AuthorizationHandler<ViewRequirement, ViewFileContext>
    {
        readonly IFileSystemAuthManager _authManager;
        private readonly ILogger _logger;

        public ViewFileHandler(IFileSystemAuthManager authManager, ILoggerFactory logFactory)
        {
            _authManager = authManager;
            _logger = logFactory.CreateLogger<ViewFileHandler>();
        }

        protected override void Handle(AuthorizationContext context, ViewRequirement requirement, ViewFileContext fileContext)
        {

            var rights = _authManager.GetFilePathAccess(context.User, fileContext.File);
            _logger.LogInformation("Got access value : " + rights);
            if ((rights & FileAccessRight.Read) > 0)
            {
                _logger.LogInformation("Allowing access");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogInformation("Denying access");
                context.Fail();
            }
        }
    }
}
