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
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
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
            long[] readerCirclesMemberships = dbContext.Circle.Include(c=>c.Members).Where(c=>c.Members.Any(m=>m.MemberId == readerId))
            .Select(c=>c.Id).ToArray();
            var result = (readerId!=null)
            ?
                 dbContext.Blogspot.Include(
                 b => b.Author
                 ).Include(p=>p.ACL).Where(x => x.Author.Id == posterId && 
                 (x.Visible && 
                 (x.ACL.Count==0 || x.ACL.Any(a=> readerCirclesMemberships.Contains(a.CircleId)))))
            :
                dbContext.Blogspot.Include(
                b => b.Author
                ).Where(x => x.Author.Id == posterId && x.Visible);
                // BlogIndexKey
            return  result;
        }
    }
}
