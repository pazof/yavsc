
using Microsoft.AspNet.Mvc;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.Data.Entity;

namespace Yavsc.Controllers
{
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
                var uid = User.GetUserId();
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
        public IActionResult ListPerformers(string id)
        {
           return Ok(dbContext.ListPerformers(billing, id));
        }
    }
}
