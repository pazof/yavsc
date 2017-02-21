
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using System.Linq;
using Yavsc.Models;
using Yavsc.Models.Workflow;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.Data.Entity;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/performers")]
    public class PerformersApiController : Controller
    {
        ApplicationDbContext dbContext;
        public PerformersApiController(ApplicationDbContext context)
        {
            dbContext = context;
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
    }
}