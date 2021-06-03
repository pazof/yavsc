using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Server.Models.EMailing;
using Microsoft.AspNet.Authorization;
using System.Security.Claims;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/mailing")]
    [Authorize("AdministratorOnly")]
    public class MailingTemplateApiController : Controller
    {
        private ApplicationDbContext _context;

        public MailingTemplateApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/MailingTemplateApi
        [HttpGet]
        public IEnumerable<MailingTemplate> GetMailingTemplate()
        {
            return _context.MailingTemplate;
        }

        // GET: api/MailingTemplateApi/5
        [HttpGet("{id}", Name = "GetMailingTemplate")]
        public async Task<IActionResult> GetMailingTemplate([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            MailingTemplate mailingTemplate = await _context.MailingTemplate.SingleAsync(m => m.Id == id);

            if (mailingTemplate == null)
            {
                return HttpNotFound();
            }

            return Ok(mailingTemplate);
        }

        // PUT: api/MailingTemplateApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMailingTemplate([FromRoute] string id, [FromBody] MailingTemplate mailingTemplate)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != mailingTemplate.Id)
            {
                return HttpBadRequest();
            }

            _context.Entry(mailingTemplate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MailingTemplateExists(id))
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

        // POST: api/MailingTemplateApi
        [HttpPost]
        public async Task<IActionResult> PostMailingTemplate([FromBody] MailingTemplate mailingTemplate)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.MailingTemplate.Add(mailingTemplate);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MailingTemplateExists(mailingTemplate.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetMailingTemplate", new { id = mailingTemplate.Id }, mailingTemplate);
        }

        // DELETE: api/MailingTemplateApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMailingTemplate([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            MailingTemplate mailingTemplate = await _context.MailingTemplate.SingleAsync(m => m.Id == id);
            if (mailingTemplate == null)
            {
                return HttpNotFound();
            }

            _context.MailingTemplate.Remove(mailingTemplate);
            await _context.SaveChangesAsync();

            return Ok(mailingTemplate);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MailingTemplateExists(string id)
        {
            return _context.MailingTemplate.Count(e => e.Id == id) > 0;
        }
    }
}
