using System.Security.Claims;

namespace Yavsc.Server.Helpers
{
    public static class UserHelpers
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("sub");
        }

        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("name");
        }

        public static bool IsSignedIn(this ClaimsPrincipal user)
        {
            return user.Identity.IsAuthenticated;
        }

    }
}
