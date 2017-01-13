using Microsoft.AspNet.Mvc;

namespace Yavsc.ApiControllers
{
    using Models;
    [Produces("application/json"),Route("api/profile")]
    public abstract class ProfileApiController<T> : Controller  
    {
        ApplicationDbContext dbContext;
        public ProfileApiController(ApplicationDbContext context)
        {
            dbContext = context;
        }

    }
}