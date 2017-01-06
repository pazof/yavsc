using Microsoft.AspNet.Mvc;

namespace Yavsc.ApiControllers
{
    using Models;
    using Models.Workflow;
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