using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yavsc.Blogspot;
using Yavsc.Models;
using Yavsc.Models.Blog;

namespace Yavsc.Server.Helpers
{
    public static class UserHelpers
    {
        public static IEnumerable<BlogPost> UserPosts(this ApplicationDbContext dbContext, string posterId, string? readerId)
        {
            if (readerId == null)
            {
                var userPosts = dbContext.blogSpotPublications.Include(
                b => b.BlogPost
                ).Where(x => x.BlogPost.AuthorId == posterId)
                .Select(x => x.BlogPost).ToArray();
                return userPosts;
            }
            else
            {
                long[] readerCirclesMemberships =
                dbContext.Circle.Include(c => c.Members)
                .Where(c => c.Members.Any(m => m.MemberId == readerId))
                .Select(c => c.Id).ToArray();
                // Mirror of BlogSpotService.Index for an
                // authenticated reader: Private restricts to the
                // author; Public is read-through-ACL.
                return dbContext.BlogSpot.Include(
                              b => b.Author
                              ).Include(p => p.ACL).Where(x => x.Author.Id == posterId &&
                              (
                                  (x.Visibility == Visibility.Private && x.AuthorId == readerId)
                                  || (x.Visibility == Visibility.Public
                                      && (x.ACL == null
                                          || x.ACL.Count == 0
                                          || x.AuthorId == readerId
                                          || (readerCirclesMemberships != null
                                              && x.ACL.Any(a => readerCirclesMemberships.Contains(a.CircleId)))))
                              ));
            }
        }

        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("sub")
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("nameid");
        }

        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("name");
        }

        public static bool IsSignedIn(this ClaimsPrincipal user)
        {
            return user.Identity.IsAuthenticated;
        }

        public static bool IsInMsRole(this ClaimsPrincipal user, string roleName)
        {
            return user.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", roleName);
        }

    }
}
