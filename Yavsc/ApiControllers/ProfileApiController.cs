using System.Linq;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Yavsc.Models;
using Yavsc.Models.Workflow;

namespace Yavsc.ApiControllers
{
    [Produces("application/json"),Route("api/profile")]
    public abstract class ProfileApiController<T> : Controller  where T : PerformerProfile
    {
        ApplicationDbContext dbContext;
        public ProfileApiController(ApplicationDbContext context)
        {
            dbContext = context;
        }


    }
}