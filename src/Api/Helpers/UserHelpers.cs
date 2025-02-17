using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Blog;

namespace Yavsc.Api.Helpers
{
    public static class UserHelpers
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        }
    }
}
