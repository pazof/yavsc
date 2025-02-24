
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Yavsc.Controllers
{
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Yavsc.Helpers;
    using Yavsc.Services;

    [Produces("application/json")]
    [Route("api/performers")]
    public class PerformersApiController : Controller
    {
        ApplicationDbContext dbContext;
        private readonly IBillingService billing;

        public PerformersApiController(ApplicationDbContext context, IBillingService billing)
        {
            dbContext = context;
            this.billing = billing;
        }

        /// <summary>
        /// Lists profiles on an activity code
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles="Performer"),HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var pfr = dbContext.Performers.Include(
                p=>p.OrganizationAddress
            ).Include(
                p=>p.Performer
            ).Include(
                p=>p.Performer.Posts
            ).SingleOrDefault(p=> p.PerformerId == id);
            if (id==null)
            {
                ModelState.AddModelError("id","Specifier un identifiant de prestataire valide");
            }
            else {
                var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!User.IsInRole("Administrator"))
                if (uid != id) return new ChallengeResult();

                if (!pfr.Active)
                    {
                        ModelState.AddModelError("id","Prestataire désactivé.");
                    }
            }
            if (ModelState.IsValid) return Ok(pfr);
            return new BadRequestObjectResult(ModelState);
        }

        [HttpGet("doing/{id}"),AllowAnonymous]
        public async Task<IActionResult> ListPerformers(string id)
        {
           return Ok(await dbContext.ListPerformersAsync(billing, id));
        }
    }
}
