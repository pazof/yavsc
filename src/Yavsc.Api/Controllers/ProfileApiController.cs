using Microsoft.AspNetCore.Mvc;

namespace Yavsc.ApiControllers
{
    /// <summary>
    /// Base class for managing performers profiles
    /// </summary>
    [Produces("application/json"),Route(Constants.APIPrefix + "/profile")]
    public abstract class ProfileApiController<T> : Controller
    {        public ProfileApiController()
        {
        }

    }
}
