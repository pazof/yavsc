using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Blog;
using Yavsc.Server.Helpers;

namespace Yavsc.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route(Constants.APIPrefix + "/estimate"), Authorize]
    public class EstimateApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        public EstimateApiController(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<EstimateApiController>();
        }
        bool UserIsAdminOrThis(string uid)
        {
            if (User.IsInRole(Constants.AdminGroupName)) return true;
            return uid == User.GetUserId();
        }
        bool UserIsAdminOrInThese(string oid, string uid)
        {
            if (User.IsInRole(Constants.AdminGroupName)) return true;
            var cuid = User.GetUserId();
            return cuid == uid || cuid == oid;
        }
        // GET: api/Estimate{?ownerId=User.GetUserId()}
        [HttpGet]
        public IActionResult GetEstimates(string? ownerId = null)
        {
            if (ownerId == null) ownerId = User.GetUserId();
            else if (!UserIsAdminOrThis(ownerId)) // throw new Exception("Not authorized") ;
                                                  // or just do nothing
                return new StatusCodeResult(StatusCodes.Status403Forbidden);
            return Ok(_context.Estimates.Include(e => e.Bill).Include(e => e.Signatures).Where(e => e.OwnerId == ownerId));
        }
        // GET: api/Estimate/5
        [HttpGet("{id}", Name = "GetEstimate")]
        public IActionResult GetEstimate([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Estimate estimate = _context.Estimates.Include(e => e.Bill).Include(e => e.Signatures).Single(m => m.Id == id);

            if (estimate == null)
            {
                return NotFound();
            }

            if (UserIsAdminOrInThese(estimate.ClientId, estimate.OwnerId))
                return Ok(estimate);
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        // PUT: api/Estimate/5
        [HttpPut("{id}"), Produces("application/json")]
        public IActionResult PutEstimate(long id, [FromBody] Estimate estimate)
        {

            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            if (id != estimate.Id)
            {
                return BadRequest();
            }
            var uid = User.GetUserId();
            if (!User.IsInRole(Constants.AdminGroupName))
            {
                if (uid != estimate.OwnerId)
                {
                    ModelState.AddModelError("OwnerId", "You can only modify your own estimates");
                    return BadRequest(ModelState);
                }
            }

            var entry = _context.Attach(estimate);
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstimateExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { estimate.Id });
        }

        // POST: api/Estimate
        [HttpPost, Produces("application/json")]
        public IActionResult PostEstimate([FromBody] Estimate estimate)
        {
            var uid = User.GetUserId();
            if (estimate.OwnerId == null) estimate.OwnerId = uid;

            if (!User.IsInRole(Constants.AdminGroupName))
            {
                if (uid != estimate.OwnerId)
                {
                    ModelState.AddModelError("OwnerId", "You can only create your own estimates");
                    return BadRequest(ModelState);
                }
            }

            if (estimate.CommandId != null)
            {
                var query = _context.NominativeServiceCommands
                    .FirstOrDefault(q => q.Id == estimate.CommandId);
                if (query == null)
                {
                    return BadRequest(ModelState);
                }
                query.ValidationDate = DateTime.UtcNow;
                _context.SaveChanges(User.GetUserId());
                _context.Entry(query).State = EntityState.Detached;
            }
            if (!ModelState.IsValid)
            {
                _logger.LogError(JsonConvert.SerializeObject(ModelState));
                return Json(ModelState);
            }
            _context.Estimates.Add(estimate);


            /* _context.AttachRange(estimate.Bill);
             _context.Attach(estimate);
             _context.Entry(estimate).State = EntityState.Added;
             foreach (var line in estimate.Bill)
                 _context.Entry(line).State = EntityState.Added;
             // foreach (var l in estimate.Bill) _context.Attach<CommandLine>(l);
            */
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (EstimateExists(estimate.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }
            return Ok(new { estimate.Id, estimate.Bill });
        }

        // DELETE: api/Estimate/5
        [HttpDelete("{id}")]
        public IActionResult DeleteEstimate(long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Estimate estimate = _context.Estimates.Include(e => e.Bill).Single(m => m.Id == id);

            if (estimate == null)
            {
                return NotFound();
            }
            var uid = User.GetUserId();
            if (!User.IsInRole(Constants.AdminGroupName))
            {
                if (uid != estimate.OwnerId)
                {
                    ModelState.AddModelError("OwnerId", "You can only create your own estimates");
                    return BadRequest(ModelState);
                }
            }
            _context.Estimates.Remove(estimate);
            _context.SaveChanges(User.GetUserId());

            return Ok(estimate);
        }

        // GET: api/estimate/asclient
        // Ongoing estimates (not yet validated by the client) where the
        // current user is the client who initiated the request.
        // An estimate only becomes visible to the client once the provider
        // has actually established and submitted it (ProviderValidationDate);
        // a freshly inserted request the provider hasn't worked on yet is
        // not something the client can validate, so it's excluded.
        [HttpGet("asclient")]
        public IActionResult GetOngoingEstimatesAsClient()
        {
            var uid = User.GetUserId();
            return Ok(_context.Estimates.Include(e => e.Bill).Include(e => e.Signatures)
                .Where(e => e.ClientId == uid
                    && e.ProviderValidationDate != default
                    && e.ClientValidationDate == default));
        }

        // GET: api/estimate/asprovider
        // Estimates awaiting the provider's signature, established by the
        // current user as the provider. Once the provider validates
        // (ProviderValidationDate stamped), the estimate leaves this list
        // and appears in the client's "awaiting client signature" list.
        [HttpGet("asprovider")]
        public IActionResult GetOngoingEstimatesAsProvider()
        {
            var uid = User.GetUserId();
            return Ok(_context.Estimates.Include(e => e.Bill).Include(e => e.Signatures)
                .Where(e => e.OwnerId == uid && e.ProviderValidationDate == default));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: api/v1/estimate/{id}/attachments
        //
        // Liste les fichiers de l'espace perso du fournisseur rattachés au
        // devis, par référence (UploadedFile). Lecture ouverte aux deux
        // parties (fournisseur/client) du devis et aux admin, comme
        // GetEstimate : la contrepartie doit pouvoir voir et télécharger
        // les pièces (le téléchargement des octets reste servi par le host
        // Blogs, qui autorise via cette même table de jointure).
        [HttpGet("{id}/attachments")]
        public IActionResult GetAttachments(long id)
        {
            var estimate = _context.Estimates.FirstOrDefault(e => e.Id == id);
            if (estimate == null) return NotFound();
            if (!UserIsAdminOrInThese(estimate.ClientId, estimate.OwnerId))
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            var attached = _context.EstimateAttachedFiles
                .Where(a => a.EstimateId == id)
                .Select(a => new
                {
                    fileId = a.FileId,
                    path = a.File.Path,
                    size = a.File.Length,
                    contentType = a.File.ContentType,
                    ownerId = a.File.OwnerId
                })
                .ToList();
            return Ok(attached);
        }

        // POST: api/v1/estimate/{id}/attachments   { fileId }
        //
        // Attache un fichier de l'espace perso du fournisseur au devis, par
        // référence. Réservé au fournisseur (estimate.OwnerId == caller) :
        // seul le fournisseur constitue le devis et y joint ses documents
        // (dont les documents à faire signer par le client, per README).
        // Le fichier doit appartenir au fournisseur (UploadedFile.OwnerId).
        [HttpPost("{id}/attachments")]
        public IActionResult AttachFile(long id, [FromBody] AttachFileRequest body)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var estimate = _context.Estimates.FirstOrDefault(e => e.Id == id);
            if (estimate == null) return NotFound();

            var uid = User.GetUserId();
            if (!User.IsInRole(Constants.AdminGroupName) && uid != estimate.OwnerId)
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            if (body == null || body.FileId <= 0)
                return BadRequest(new { error = "fileId required" });

            var file = _context.UploadedFiles.FirstOrDefault(f => f.Id == body.FileId);
            if (file == null) return BadRequest(new { error = "file not found" });
            if (file.OwnerId != uid)
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            var existing = _context.EstimateAttachedFiles
                .FirstOrDefault(a => a.EstimateId == id && a.FileId == body.FileId);
            if (existing != null) return Ok(new { estimateId = id, fileId = body.FileId });

            _context.EstimateAttachedFiles.Add(new EstimateAttachedFile
            {
                EstimateId = id,
                FileId = body.FileId
            });
            _context.SaveChanges(User.GetUserId());
            return Ok(new { estimateId = id, fileId = body.FileId });
        }

        // DELETE: api/v1/estimate/{id}/attachments/{fileId}
        //
        // Détache un fichier du devis. Réservé au fournisseur.
        [HttpDelete("{id}/attachments/{fileId}")]
        public IActionResult DetachFile(long id, long fileId)
        {
            var estimate = _context.Estimates.FirstOrDefault(e => e.Id == id);
            if (estimate == null) return NotFound();

            var uid = User.GetUserId();
            if (!User.IsInRole(Constants.AdminGroupName) && uid != estimate.OwnerId)
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            var link = _context.EstimateAttachedFiles
                .FirstOrDefault(a => a.EstimateId == id && a.FileId == fileId);
            if (link == null) return NotFound();
            _context.EstimateAttachedFiles.Remove(link);
            _context.SaveChanges(User.GetUserId());
            return Ok(new { estimateId = id, fileId });
        }

        private bool EstimateExists(long id)
        {
            return _context.Estimates.Count(e => e.Id == id) > 0;
        }
    }
}
