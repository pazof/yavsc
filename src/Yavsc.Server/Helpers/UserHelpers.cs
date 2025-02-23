using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Blog;

namespace Yavsc.Helpers
{
    public static class UserHelpers
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("sub");
        }

        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Name);
        }

        public static bool IsSignedIn(this ClaimsPrincipal user)
        {
            return user.Identity.IsAuthenticated;
        }

        public static IEnumerable<BlogPost> UserPosts(this ApplicationDbContext dbContext, string posterId, string readerId)
        {
            if (readerId == null)
            {
                var userPosts = dbContext.BlogSpot.Include(
                b => b.Author
                ).Where(x => ((x.AuthorId == posterId))).ToArray();
                return userPosts;
            }
            else
            {
                long[] readerCirclesMemberships =
                dbContext.Circle.Include(c => c.Members)
                .Where(c => c.Members.Any(m => m.MemberId == readerId))
                .Select(c => c.Id).ToArray();
                return dbContext.BlogSpot.Include(
                              b => b.Author
                              ).Include(p => p.ACL).Where(x => x.Author.Id == posterId &&
                              (x.ACL.Count == 0 || x.ACL.Any(a => readerCirclesMemberships.Contains(a.CircleId))));


            }

        }
    }
}
