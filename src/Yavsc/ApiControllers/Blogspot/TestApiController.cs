using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace Yavsc.ApiControllers
{
    using System.ComponentModel.DataAnnotations;
    using Yavsc.Attributes.Validation;

    [Authorize,Route("~/api/test")]
    public class TestApiController : Controller
    {
        
    }

}
