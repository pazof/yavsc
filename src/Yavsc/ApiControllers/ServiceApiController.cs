using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Market;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/ServiceApi")]
    public class ServiceApiController : Controller
    {
        private ApplicationDbContext _context;

        public ServiceApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ServiceApi
        [HttpGet]
        public IEnumerable<Service> GetServices()
        {
            return _context.Services;
        }

        // GET: api/ServiceApi/5
        [HttpGet("{id}", Name = "GetService")]
        public IActionResult GetService([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            Service service = _context.Services.Single(m => m.Id == id);

            if (service == null)
            {
                return HttpNotFound();
            }

            return Ok(service);
        }

        // PUT: api/ServiceApi/5
        [HttpPut("{id}"),Authorize(Constants.FrontOfficeGroupName)]
        public IActionResult PutService(long id, [FromBody] Service service)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != service.Id)
            {
                return HttpBadRequest();
            }

            _context.Entry(service).State = EntityState.Modified;

            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(id))
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

        // POST: api/ServiceApi
        [HttpPost,Authorize(Constants.FrontOfficeGroupName)]
        public IActionResult PostService([FromBody] Service service)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.Services.Add(service);
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (ServiceExists(service.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetService", new { id = service.Id }, service);
        }

        // DELETE: api/ServiceApi/5
        [HttpDelete("{id}"),Authorize(Constants.FrontOfficeGroupName)]
        public IActionResult DeleteService(long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            Service service = _context.Services.Single(m => m.Id == id);
            if (service == null)
            {
                return HttpNotFound();
            }

            _context.Services.Remove(service);
            _context.SaveChanges(User.GetUserId());

            return Ok(service);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ServiceExists(long id)
        {
            return _context.Services.Count(e => e.Id == id) > 0;
        }
    }
}