
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Blog;
using Yavsc.Helpers;
using System.Security.Claims;
using IdentityServer8.Extensions;

namespace Yavsc.ViewComponents
{
    public class BlogIndexViewComponent: ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public BlogIndexViewComponent(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // Renders blog index ofr the specified user by name,
        // grouped by title
        public async Task<IViewComponentResult> InvokeAsync(int skip=0, int maxLen=25)
        {
            IEnumerable<BlogPost> posts;

            if (User.IsAuthenticated())
            {
                string viewerId = UserClaimsPrincipal.GetUserId();
                long[] usercircles = await _context.Circle.Include(c=>c.Members).
                    Where(c=>c.Members.Any(m=>m.MemberId == viewerId))
                    .Select(c=>c.Id).ToArrayAsync();
                
                IQueryable<BlogPost> allposts = _context.BlogSpot
                    .Include(b => b.Author)
                    .Include(p=>p.ACL)
                    .Include(p=>p.Tags)
                    .Include(p=>p.Comments)
                    .Where(p => p.AuthorId == viewerId || p.Visible);

                posts = (usercircles != null) ? 
                    allposts.Where(p=> p.ACL.Count==0 || p.ACL.Any(a => usercircles.Contains(a.CircleId)))
                    : allposts.Where(p => p.ACL.Count == 0);

            }
            else 
            {
                 posts = _context.BlogSpot
                .Include(b => b.Author)
                .Include(p=>p.ACL)
                .Include(p=>p.Tags)
                .Include(p=>p.Comments)
                .Where(p => p.Visible && p.ACL.Count == 0 ).ToArray();
            }
          
            var data = posts.OrderByDescending( p=> p.DateCreated);
            var grouped = data.GroupBy(p=> p.Title).Skip(skip).Take(maxLen);

            return View("Default", grouped);
        }
    }
}
