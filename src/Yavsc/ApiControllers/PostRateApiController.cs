using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yavsc.Helpers;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("~/api/PostRateApi")]
    public class PostRateApiController : Controller
    {
        private readonly ApplicationDbContext _context;

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
                return BadRequest(ModelState);
            }

            Models.Blog.BlogPost blogpost = _context.Blogspot.Single(x=>x.Id == id);

            if (blogpost == null)
            {
                return NotFound();
            }

            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (blogpost.AuthorId!=uid)
                if (!User.IsInRole(Constants.AdminGroupName))
                    return BadRequest();

            blogpost.Rate = rate;
            _context.SaveChanges(User.GetUserId());

            return Ok();
        }
    }
}
