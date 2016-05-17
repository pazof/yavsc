
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace Yavsc.Controllers
{
    [Authorize, ServiceFilter(typeof(LanguageActionFilter))]
    public class UserFilesController : Controller
    {
    }
}