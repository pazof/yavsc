using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Access;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/blacklist"), Authorize]
    public class BlackListApiController : Controller
    {
        private ApplicationDbContext _context;

        public BlackListApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/BlackListApi
        [HttpGet]
        public IEnumerable<BlackListed> GetBlackListed()
        {
            return _context.BlackListed;
        }

        // GET: api/BlackListApi/5
        [HttpGet("{id}", Name = "GetBlackListed")]
        public IActionResult GetBlackListed([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }
            BlackListed blackListed = _context.BlackListed.Single(m => m.Id == id);
            if (blackListed == null)
            {
                return HttpNotFound();
            }
            if (!CheckPermission(blackListed))
                return HttpBadRequest();

            return Ok(blackListed);
        }

        private bool CheckPermission(BlackListed blackListed)
        {
            var uid = User.GetUserId();
            if (uid != blackListed.OwnerId)
                if (!User.IsInRole(Constants.AdminGroupName))
                    if (!User.IsInRole(Constants.FrontOfficeGroupName))
                        return false;
            return true;
        }
        // PUT: api/BlackListApi/5
        [HttpPut("{id}")]
        public IActionResult PutBlackListed(long id, [FromBody] BlackListed blackListed)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != blackListed.Id)
            {
                return HttpBadRequest();
            }
            if (!CheckPermission(blackListed))
                return HttpBadRequest();
            _context.Entry(blackListed).State = EntityState.Modified;

            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlackListedExists(id))
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

        // POST: api/BlackListApi
        [HttpPost]
        public IActionResult PostBlackListed([FromBody] BlackListed blackListed)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (!CheckPermission(blackListed))
                return HttpBadRequest();

            _context.BlackListed.Add(blackListed);
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (BlackListedExists(blackListed.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetBlackListed", new { id = blackListed.Id }, blackListed);
        }

        // DELETE: api/BlackListApi/5
        [HttpDelete("{id}")]
        public IActionResult DeleteBlackListed(long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            BlackListed blackListed = _context.BlackListed.Single(m => m.Id == id);
            if (blackListed == null)
            {
                return HttpNotFound();
            }

            if (!CheckPermission(blackListed))
                return HttpBadRequest();
                
            _context.BlackListed.Remove(blackListed);
            _context.SaveChanges(User.GetUserId());

            return Ok(blackListed);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BlackListedExists(long id)
        {
            return _context.BlackListed.Count(e => e.Id == id) > 0;
        }
    }
}