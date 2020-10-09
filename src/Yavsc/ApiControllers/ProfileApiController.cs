using Microsoft.AspNet.Mvc;

namespace Yavsc.ApiControllers
{
    using Models;

    /// <summary>
    /// Base class for managing performers profiles
    /// </summary>
    [Produces("application/json"),Route("api/profile")]
    public abstract class ProfileApiController<T> : Controller  
    {        public ProfileApiController()
        {
        }

    }
}
