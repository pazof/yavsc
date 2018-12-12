using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Authorization;
using Microsoft.Extensions.OptionsModel;
using Yavsc.Models;
using Microsoft.Data.Entity;
using System.Linq;
using Yavsc.Models.Blog;

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
        public async Task<IViewComponentResult> InvokeAsync(string viewerId, int skip=0, int maxLen=25)
        {
            long[] usercircles = _context.Circle.Include(c=>c.Members).
            Where(c=>c.Members.Any(m=>m.MemberId == viewerId))
            .Select(c=>c.Id).ToArray();
            IQueryable<BlogPost> posts ;
            var allposts = _context.Blogspot
                .Include(b => b.Author)
                .Include(p=>p.ACL)
                .Include(p=>p.Tags)
                .Include(p=>p.Comments)
                .Where(p=>p.AuthorId == viewerId || p.Visible);

            if (usercircles != null) {
                posts = allposts.Where(p=> p.ACL.Count==0 || p.ACL.Any(a=> usercircles.Contains(a.CircleId)))
                ;
            }
            else {
                posts = allposts.Where(p => p.ACL.Count == 0);
            }

            var data = posts.OrderByDescending( p=> p.DateCreated).ToArray();
            var grouped = data.GroupBy(p=> p.Title).Skip(skip).Take(maxLen);

            return View("Default", grouped);
        }
    }
}
