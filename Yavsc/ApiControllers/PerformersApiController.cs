
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using System.Linq;
using Yavsc.Models;
using Yavsc.Models.Workflow;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;

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
        [Authorize(Roles="Performer")]
        public IActionResult Get(string id)
        {
            if (id==null)
            {
                ModelState.AddModelError("id","Specifier un code activité");
                return new BadRequestObjectResult(ModelState);
            }
            return Ok(dbContext.Performers.Where(p=>p.Active && p.PerformerId == id));
        }
    }
}