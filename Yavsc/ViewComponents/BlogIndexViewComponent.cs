using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Authorization;
using Microsoft.Extensions.OptionsModel;
using Yavsc.Models;

namespace Yavsc.ViewComponents
{
    public class BlogIndexViewComponent: ViewComponent
    {
        ILogger _logger;
        private ApplicationDbContext _context;
        private IAuthorizationService _authorizationService;
        public BlogIndexViewComponent(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILoggerFactory loggerFactory,
            IAuthorizationService authorizationService,
            IOptions<SiteSettings> siteSettings)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<BlogIndexViewComponent>();
            _authorizationService = authorizationService;
        }

        // Renders blog index ofr the specified user by name
        public async Task<IViewComponentResult> InvokeAsync(string userName)
        {
            return View("Default");
        }
    }
}