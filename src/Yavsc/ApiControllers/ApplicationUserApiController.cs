using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    [Produces("application/json"),Authorize(Roles="Administrator")]
    [Route("api/users")]
    public class ApplicationUserApiController : Controller
    {
        private ApplicationDbContext _context;

        public ApplicationUserApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ApplicationUserApi
        [HttpGet]
        public IEnumerable<ApplicationUser> GetApplicationUser()
        {
            return _context.Users.Include(u=>u.Roles).Include(u=>u.Logins).Include(u=>u.Claims);
        }

        // GET: api/ApplicationUserApi/5
        [HttpGet("{id}", Name = "GetApplicationUser")]
        public IActionResult GetApplicationUser([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            ApplicationUser applicationUser = _context.Users.Single(m => m.Id == id);

            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            return Ok(applicationUser);
        }

        // PUT: api/ApplicationUserApi/5
        [HttpPut("{id}")]
        public IActionResult PutApplicationUser(string id, [FromBody] ApplicationUser applicationUser)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != applicationUser.Id)
            {
                return HttpBadRequest();
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
                    return HttpNotFound();
                }
                else
                {
                    throw;
                }
            }

            return new HttpStatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/ApplicationUserApi
        [HttpPost]
        public IActionResult PostApplicationUser([FromBody] ApplicationUser applicationUser)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
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
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
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
                return HttpBadRequest(ModelState);
            }

            ApplicationUser applicationUser = _context.Users.Single(m => m.Id == id);
            if (applicationUser == null)
            {
                return HttpNotFound();
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