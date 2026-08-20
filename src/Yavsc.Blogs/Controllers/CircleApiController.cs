using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Relationship;
using Yavsc.Server.Helpers;
using static Yavsc.Blogs.Constants;

namespace Yavsc.Blogs.Controllers
{
    [Produces("application/json")]
    [Route(APIPrefix +"/circle")]
    public class CircleApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CircleApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the caller's own circles. Circles are personal —
        /// the API never exposes another user's circles, even by id.
        /// </summary>
        // GET: api/circle
        [HttpGet]
        public IEnumerable<Circle> GetCircle()
        {
            var uid = User.GetUserId();
            return _context.Circle.Where(c => c.OwnerId == uid);
        }

        /// <summary>
        /// Returns a single circle only when it belongs to the caller.
        /// </summary>
        // GET: api/circle/5
        [HttpGet("{id}", Name = "GetCircle")]
        public async Task<IActionResult> GetCircle([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uid = User.GetUserId();
            Circle circle = await _context.Circle.SingleOrDefaultAsync(
                m => m.Id == id && m.OwnerId == uid);

            if (circle == null)
            {
                return NotFound();
            }

            return Ok(circle);
        }

        /// <summary>
        /// Replaces a circle. The caller must own it; the server
        /// reasserts ownership regardless of any <c>OwnerId</c>
        /// the client tries to put in the body.
        ///
        /// <para>The body shape is a <see cref="CircleDto"/> — a
        /// flat, navigation-free projection — not the EF entity.
        /// The EF entity carries <c>[JsonIgnore]</c>-decorated
        /// navigation properties (<c>Owner</c>, <c>Members</c>)
        /// that bind to server-only types (<c>ApplicationUser</c>,
        /// <c>CircleMember</c>); keeping the wire shape as a
        /// DTO avoids any future regression where the entity
        /// grows a navigable property that System.Text.Json
        /// refuses to materialise. The client-side mirror lives
        /// in <c>Yavsc.Api.Client.Dtos.CircleDto</c>.</para>
        /// </summary>
        // PUT: api/circle/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCircle(
            [FromRoute] long id,
            [FromBody] CircleDto circle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != circle.Id)
            {
                return BadRequest();
            }

            var uid = User.GetUserId();
            var existing = await _context.Circle.SingleOrDefaultAsync(
                c => c.Id == id && c.OwnerId == uid);
            if (existing is null)
            {
                return new ChallengeResult();
            }

            // Map the wire shape onto the entity. OwnerId is
            // forced to the caller regardless of what the body
            // says; Name and Public come from the body.
            existing.Name = circle.Name;
            existing.Public = circle.Public;
            existing.OwnerId = uid;

            _context.Entry(existing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CircleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        /// <summary>
        /// Creates a circle owned by the caller. The server overwrites
        /// any OwnerId the client sends in the body.
        /// </summary>
        // POST: api/circle
        [HttpPost]
        public async Task<IActionResult> PostCircle([FromBody] CircleDto circle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uid = User.GetUserId();
            circle.OwnerId = uid;
            Circle newCircle = new Circle
            {
                OwnerId = User.GetUserId(),
                Name = circle.Name,
                Public = circle.Public
            };

            _context.Circle.Add(newCircle);
            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (CircleExists(circle.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetCircle", new { id = circle.Id }, circle);
        }

        /// <summary>
        /// Deletes a circle only if the caller owns it. Returns 404
        /// (not 403) when the circle does not exist or is not owned
        /// by the caller, to avoid leaking the existence of someone
        /// else's circle.
        /// </summary>
        // DELETE: api/circle/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCircle([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uid = User.GetUserId();
            Circle circle = await _context.Circle.SingleOrDefaultAsync(
                m => m.Id == id && m.OwnerId == uid);
            if (circle == null)
            {
                return NotFound();
            }

            _context.Circle.Remove(circle);
            await _context.SaveChangesAsync(User.GetUserId());

            return Ok(circle);
        }

        /// <summary>
        /// Returns the members of one of the caller's circles.
        /// Returns 404 (not 403) when the circle does not exist
        /// or is not owned by the caller, mirroring the scoping
        /// of the rest of this controller.
        /// </summary>
        // GET: api/circle/5/members
        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetMembers([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uid = User.GetUserId();
            var ownsIt = await _context.Circle.AnyAsync(c => c.Id == id && c.OwnerId == uid);
            if (!ownsIt)
            {
                return NotFound();
            }

            var members = await _context.CircleMembers
                .Where(m => m.CircleId == id)
                .Select(m => new CircleMemberDto
                {
                    Id = m.MemberId,
                    UserName = m.Member.UserName ?? string.Empty,
                    FullName = m.Member.FullName,
                    Avatar = m.Member.Avatar,
                })
                .ToListAsync();

            return Ok(members);
        }

        /// <summary>
        /// Adds a Yavsc user to one of the caller's circles. The
        /// body carries the user id (resolved client-side via the
        /// central <c>/api/user-search</c> endpoint). Returns
        /// 404 (not 403) when the circle does not exist or is not
        /// owned by the caller, and 404 when the target user does
        /// not exist, so the caller can't probe whether an email
        /// belongs to a real account.
        ///
        /// <para>Returns 409 Conflict if the user is already a
        /// member of the circle; the client treats this as a
        /// no-op success.</para>
        /// </summary>
        // POST: api/circle/5/members
        // body: { "userId": "..." }
        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(
            [FromRoute] long id,
            [FromBody] AddCircleMemberDto body)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uid = User.GetUserId();
            var ownsIt = await _context.Circle.AnyAsync(c => c.Id == id && c.OwnerId == uid);
            if (!ownsIt)
            {
                return NotFound();
            }

            // Reject unknown user ids the same way as an unknown
            // circle: 404. Probing the user table by id should not
            // be possible through this endpoint.
            var userExists = await _context.Users.AnyAsync(u => u.Id == body.UserId);
            if (!userExists)
            {
                return NotFound();
            }

            // Idempotency: re-adding an existing member is a
            // 409, not a silent success. Clients that don't
            // dedupe beforehand will at least get an actionable
            // status code rather than a misleading "created".
            var alreadyMember = await _context.CircleMembers.AnyAsync(
                m => m.CircleId == id && m.MemberId == body.UserId);
            if (alreadyMember)
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            _context.CircleMembers.Add(new CircleMember
            {
                CircleId = id,
                MemberId = body.UserId,
            });
            await _context.SaveChangesAsync(User.GetUserId());

            return CreatedAtRoute("GetCircle", new { id }, body);
        }

        /// <summary>
        /// Removes a user from one of the caller's circles.
        /// Returns 404 when the circle does not exist or is not
        /// owned by the caller, mirroring the rest of this
        /// controller's scoping. Returns 404 when the user is
        /// not a member of the circle (idempotent: removing a
        /// non-member is the same as having nothing to remove).
        /// </summary>
        // DELETE: api/circle/5/members/tester
        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(
            [FromRoute] long id,
            [FromRoute] string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uid = User.GetUserId();
            var ownsIt = await _context.Circle.AnyAsync(c => c.Id == id && c.OwnerId == uid);
            if (!ownsIt)
            {
                return NotFound();
            }

            var membership = await _context.CircleMembers.SingleOrDefaultAsync(
                m => m.CircleId == id && m.MemberId == userId);
            if (membership is null)
            {
                return NotFound();
            }

            _context.CircleMembers.Remove(membership);
            await _context.SaveChangesAsync(User.GetUserId());

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CircleExists(long id)
        {
            return _context.Circle.Count(e => e.Id == id) > 0;
        }
    }

    /// <summary>
    /// Wire shape for <c>PUT /api/circle/{id}</c>. Flat by
    /// design — navigation properties (<c>Owner</c>,
    /// <c>Members</c>) live on the EF entity only and never
    /// cross the wire.
    ///
    /// <para>Field names match the JSON the server emits
    /// (camelCase via ASP.NET Core's Web defaults), so no
    /// <c>[JsonPropertyName]</c> attributes are required.
    /// Mirrors the client-side <c>Yavsc.Api.Client.Dtos.CircleDto</c>
    /// — keep them in sync.</para>
    /// </summary>
    public sealed class CircleDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public bool Public { get; set; }
    }

    /// <summary>
    /// Wire shape for <c>GET /api/circle/{id}/members</c>.
    /// Mirrors <see cref="UserSearchResultDto"/> but stops
    /// short of the Email field — circle membership UI only
    /// needs to render a name and an avatar, not contact
    /// details.
    /// </summary>
    public sealed class CircleMemberDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
    }

    /// <summary>
    /// Wire shape for <c>POST /api/circle/{id}/members</c>.
    /// The body is intentionally tiny: the client resolves
    /// the user id via <c>/api/user-search</c> before
    /// posting, so all we need is the resolved id.
    /// </summary>
    public sealed class AddCircleMemberDto
    {
        public string UserId { get; set; } = string.Empty;
    }
}
