using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Server.Models.IT.SourceCode;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/GitRefsApi")]
    [Authorize("AdministratorOnly")]
    public class GitRefsApiController : Controller
    {
        private ApplicationDbContext _context;

        public GitRefsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/GitRefsApi
        [HttpGet]
        public IEnumerable<GitRepositoryReference> GetGitRepositoryReference()
        {
            return _context.GitRepositoryReference;
        }

        // GET: api/GitRefsApi/5
        [HttpGet("{id}", Name = "GetGitRepositoryReference")]
        public async Task<IActionResult> GetGitRepositoryReference([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            GitRepositoryReference gitRepositoryReference = await _context.GitRepositoryReference.SingleAsync(m => m.Id == id);

            if (gitRepositoryReference == null)
            {
                return HttpNotFound();
            }

            return Ok(gitRepositoryReference);
        }

        // PUT: api/GitRefsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGitRepositoryReference([FromRoute] long id, [FromBody] GitRepositoryReference gitRepositoryReference)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.Entry(gitRepositoryReference).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GitRepositoryReferenceExists(id))
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

        // POST: api/GitRefsApi
        [HttpPost]
        public async Task<IActionResult> PostGitRepositoryReference([FromBody] GitRepositoryReference gitRepositoryReference)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.GitRepositoryReference.Add(gitRepositoryReference);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (GitRepositoryReferenceExists(gitRepositoryReference.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetGitRepositoryReference", new { id = gitRepositoryReference.Path }, gitRepositoryReference);
        }

        // DELETE: api/GitRefsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGitRepositoryReference([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            GitRepositoryReference gitRepositoryReference = await _context.GitRepositoryReference.SingleAsync(m => m.Id == id);
            if (gitRepositoryReference == null)
            {
                return HttpNotFound();
            }

            _context.GitRepositoryReference.Remove(gitRepositoryReference);
            await _context.SaveChangesAsync();

            return Ok(gitRepositoryReference);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GitRepositoryReferenceExists(long id)
        {
            return _context.GitRepositoryReference.Count(e => e.Id == id) > 0;
        }
    }
}