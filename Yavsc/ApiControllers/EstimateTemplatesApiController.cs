using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Billing;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/EstimateTemplatesApi")]
    public class EstimateTemplatesApiController : Controller
    {
        private ApplicationDbContext _context;

        public EstimateTemplatesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/EstimateTemplatesApi
        [HttpGet]
        public IEnumerable<EstimateTemplate> GetEstimateTemplate()
        {
            var uid = User.GetUserId();
            return _context.EstimateTemplates.Where(x=>x.OwnerId==uid);
        }

        // GET: api/EstimateTemplatesApi/5
        [HttpGet("{id}", Name = "GetEstimateTemplate")]
        public IActionResult GetEstimateTemplate([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }
            var uid = User.GetUserId();

            EstimateTemplate estimateTemplate = _context.EstimateTemplates.Where(x=>x.OwnerId==uid).Single(m => m.Id == id);

            if (estimateTemplate == null)
            {
                return HttpNotFound();
            }

            return Ok(estimateTemplate);
        }

        // PUT: api/EstimateTemplatesApi/5
        [HttpPut("{id}")]
        public IActionResult PutEstimateTemplate(long id, [FromBody] EstimateTemplate estimateTemplate)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != estimateTemplate.Id)
            {
                return HttpBadRequest();
            }
            var uid = User.GetUserId();
            if (estimateTemplate.OwnerId!=uid)
            if (!User.IsInRole(Constants.AdminGroupName))
            return new HttpStatusCodeResult(StatusCodes.Status403Forbidden);

            _context.Entry(estimateTemplate).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstimateTemplateExists(id))
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

        // POST: api/EstimateTemplatesApi
        [HttpPost]
        public IActionResult PostEstimateTemplate([FromBody] EstimateTemplate estimateTemplate)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }
            estimateTemplate.OwnerId=User.GetUserId();

            _context.EstimateTemplates.Add(estimateTemplate);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (EstimateTemplateExists(estimateTemplate.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetEstimateTemplate", new { id = estimateTemplate.Id }, estimateTemplate);
        }

        // DELETE: api/EstimateTemplatesApi/5
        [HttpDelete("{id}")]
        public IActionResult DeleteEstimateTemplate(long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            EstimateTemplate estimateTemplate = _context.EstimateTemplates.Single(m => m.Id == id);
            if (estimateTemplate == null)
            {
                return HttpNotFound();
            }
            var uid = User.GetUserId();
            if (estimateTemplate.OwnerId!=uid)
            if (!User.IsInRole(Constants.AdminGroupName))
            return new HttpStatusCodeResult(StatusCodes.Status403Forbidden);

            _context.EstimateTemplates.Remove(estimateTemplate);
            _context.SaveChanges();

            return Ok(estimateTemplate);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EstimateTemplateExists(long id)
        {
            return _context.EstimateTemplates.Count(e => e.Id == id) > 0;
        }
    }
}