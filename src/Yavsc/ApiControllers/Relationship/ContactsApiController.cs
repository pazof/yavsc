using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Abstract.Identity;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/ContactsApi")]
    public class ContactsApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ContactsApi
        [HttpGet("{id}")]
        public ClientProviderInfo GetClientProviderInfo(string id)
        {
            return _context.ClientProviderInfo.FirstOrDefault(c=>c.UserId == id);
        }

        // PUT: api/ContactsApi/5
        [HttpPut("{id}")]
        public IActionResult PutClientProviderInfo(string id, [FromBody] ClientProviderInfo clientProviderInfo)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != clientProviderInfo.UserId)
            {
                return HttpBadRequest();
            }

            _context.Entry(clientProviderInfo).State = EntityState.Modified;

            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientProviderInfoExists(id))
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

        // POST: api/ContactsApi
        [HttpPost]
        public IActionResult PostClientProviderInfo([FromBody] ClientProviderInfo clientProviderInfo)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.ClientProviderInfo.Add(clientProviderInfo);
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (ClientProviderInfoExists(clientProviderInfo.UserId))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetClientProviderInfo", new { id = clientProviderInfo.UserId }, clientProviderInfo);
        }

        // DELETE: api/ContactsApi/5
        [HttpDelete("{id}")]
        public IActionResult DeleteClientProviderInfo(string id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            ClientProviderInfo clientProviderInfo = _context.ClientProviderInfo.Single(m => m.UserId == id);
            if (clientProviderInfo == null)
            {
                return HttpNotFound();
            }

            _context.ClientProviderInfo.Remove(clientProviderInfo);
            _context.SaveChanges(User.GetUserId());

            return Ok(clientProviderInfo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ClientProviderInfoExists(string id)
        {
            return _context.ClientProviderInfo.Count(e => e.UserId == id) > 0;
        }
    }
}
