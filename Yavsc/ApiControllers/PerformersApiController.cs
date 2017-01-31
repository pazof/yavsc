
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using System.Linq;
using Yavsc.Models;
using Yavsc.Models.Workflow;

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
        public IEnumerable<PerformerProfile> Get()
        {
            return dbContext.Performers.Where(p=>p.Active && p.AcceptPublicContact);
        }
    }
}