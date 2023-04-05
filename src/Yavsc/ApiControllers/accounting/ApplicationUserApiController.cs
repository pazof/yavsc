using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Identity;
using Yavsc.Helpers;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    [Produces("application/json"),Authorize(Roles="Administrator")]
    [Route("api/users")]
    public class ApplicationUserApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApplicationUserApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ApplicationUserApi
        [HttpGet]
        public IEnumerable<UserInfo> GetApplicationUser(int skip=0, int take = 25)
        {
            return _context.Users.Skip(skip).Take(take)
            .Select(u=> new UserInfo{ 
            UserId = u.Id,
            UserName = u.UserName,
            Avatar = u.Avatar});
        }

        [HttpGet("search/{pattern}")]
        public IEnumerable<UserInfo> SearchApplicationUser(string pattern, int skip=0, int take = 25)
        {
            return _context.Users.Where(u => u.UserName.Contains(pattern))
            .Skip(skip).Take(take)
            .Select(u=> new UserInfo { 
            UserId = u.Id,
            UserName = u.UserName,
            Avatar = u.Avatar   });
        }

        // GET: api/ApplicationUserApi/5
        [HttpGet("{id}", Name = "GetApplicationUser")]
        public IActionResult GetApplicationUser([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApplicationUser applicationUser = _context.Users.Single(m => m.Id == id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            return Ok(applicationUser);
        }

        // PUT: api/ApplicationUserApi/5
        [HttpPut("{id}")]
        public IActionResult PutApplicationUser(string id, [FromBody] ApplicationUser applicationUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != applicationUser.Id)
            {
                return BadRequest();
            }

            _context.Entry(applicationUser).State = EntityState.Modified;

            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationUserExists(id))
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

        // POST: api/ApplicationUserApi
        [HttpPost]
        public IActionResult PostApplicationUser([FromBody] ApplicationUser applicationUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Users.Add(applicationUser);
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (ApplicationUserExists(applicationUser.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetApplicationUser", new { id = applicationUser.Id }, applicationUser);
        }

        // DELETE: api/ApplicationUserApi/5
        [HttpDelete("{id}")]
        public IActionResult DeleteApplicationUser(string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApplicationUser applicationUser = _context.Users.Single(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            _context.Users.Remove(applicationUser);
            _context.SaveChanges(User.GetUserId());

            return Ok(applicationUser);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ApplicationUserExists(string id)
        {
            return _context.Users.Count(e => e.Id == id) > 0;
        }
    }
}
