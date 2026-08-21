using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using static Yavsc.Constants;

namespace Yavsc.Blogs.Controllers
{
    /// <summary>
    /// Central user search endpoint used by client address books
    /// (PostIt.Desktop, future PostIt.Browser CLI, etc.).
    ///
    /// <para>Live in <c>Yavsc.Blogs</c> rather than <c>Yavsc.Api</c>
    /// because Yavsc.Api is not yet enabled in production; future
    /// migration is mechanical (the namespace and route prefix are
    /// the only ties to the host project).</para>
    ///
    /// <para>Authorisation: any authenticated caller can search.
    /// Results include <c>Email</c> on a best-effort basis —
    /// the field is included because the address-book use case
    /// (composing a circle membership, sending an invite) needs
    /// it. The data set is the entire user table of the
    /// instance, which on Yavsc's single-tenant deployments is
    /// a closed community where users already know each other.
    /// Multi-tenant deployments should gate this controller
    /// behind a tenant-scoped authorisation policy before
    /// exposing it.</para>
    /// </summary>
    [Produces("application/json")]
    [Route(APIPrefix + "/user-search")]
    [Authorize]
    public class UserSearchApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserSearchApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Search users by display name and/or email.
        /// </summary>
        /// <param name="q">Substring filter on
        /// <see cref="ApplicationUser.FullName"/> or
        /// <see cref="ApplicationUser.UserName"/> (case-insensitive,
        /// contains). Optional.</param>
        /// <param name="e">Exact filter on
        /// <see cref="ApplicationUser.Email"/> (case-insensitive
        /// equality). Optional.</param>
        /// <param name="take">Maximum number of results, capped at
        /// 100. Default 25.</param>
        // GET: api/user-search?q=foo&e=bar@example.com&take=25
        [HttpGet]
        public async Task<IEnumerable<UserSearchResultDto>> SearchAsync(
            [FromQuery] string? q = null,
            [FromQuery] string? e = null,
            [FromQuery] int take = 25)
        {
            take = Math.Clamp(take, 1, 100);

            IQueryable<ApplicationUser> query = _context.Users;

            if (!string.IsNullOrWhiteSpace(e))
            {
                // Email is treated as an exact match — most address
                // book callers already know the email they're
                // searching for and we don't want to surface a
                // long tail of partial matches.
                var normalized = e.Trim();
                query = query.Where(u => u.Email != null &&
                string.Compare(u.Email, normalized, true) ==0);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var needle = q.Trim();
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(needle.ToLower())) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(needle.ToLower())));
            }

            var results = await query
                .OrderBy(u => u.FullName ?? u.UserName)
                .Take(take)
                .Select(u => new UserSearchResultDto
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    FullName = u.FullName,
                    Avatar = u.Avatar,
                    Email = u.Email,
                })
                .ToListAsync();

            return results;
        }
    }

    /// <summary>
    /// Search-result shape. Flat DTO with no navigation
    /// properties so the JSON stays small even if the user
    /// table grows.
    /// </summary>
    public sealed class UserSearchResultDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public string? Email { get; set; }
    }
}
