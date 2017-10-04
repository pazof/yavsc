using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("~/api/PostRateApi")]
    public class PostRateApiController : Controller
    {
        private ApplicationDbContext _context;

        public PostRateApiController(ApplicationDbContext context)
        {
            _context = context;
        }

                // GET: api/PostRateApi/5
        [HttpPut("{id}"),Authorize]
        public IActionResult PutPostRate([FromRoute] long id, [FromBody] int rate)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            Models.Blog.BlogPost blogpost = _context.Blogspot.Single(x=>x.Id == id);

            if (blogpost == null)
            {
                return HttpNotFound();
            }

            var uid = User.GetUserId();
            if (blogpost.AuthorId!=uid)
                if (!User.IsInRole(Constants.AdminGroupName))
                    return HttpBadRequest();

            blogpost.Rate = rate;
            _context.SaveChanges(User.GetUserId());

            return Ok();
        }
    }
}
