using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yavsc.Models.Blog;
using Yavsc.Server.Exceptions;
using Yavsc.Server.Helpers;
using static Yavsc.Blogs.Constants;

namespace Yavsc.Blogs.Controllers
{
    [Authorize("BlogScope")]
    [Produces("application/json")]
    [Route(APIPrefix + "/blog")]
    public class BlogApiController : Controller
    {
        private readonly BlogSpotService blogSpotService;

        public BlogApiController(BlogSpotService blogSpotService)
        {
            this.blogSpotService = blogSpotService;
        }

        // GET: api/BlogApi
        [HttpGet]
        public async Task<IEnumerable<IBlogPost>> GetBlogspot(int start = 0, int take = 25)
        {
            return await blogSpotService.Index(User, null, start, take);
        }

        // GET: api/BlogApi/5
        [HttpGet("{id}", Name = "GetBlog")]
        public async Task<IActionResult> GetBlog([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var blog = await blogSpotService.Details(User, id);
                if (blog == null)
                {
                    return NotFound();
                }

                return Ok(blog);
            }
            catch (AuthorizationFailureException)
            {
                return Challenge();
            }
        }

        // PUT: api/BlogApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBlog(long id, [FromBody] BlogPost blog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != blog.Id)
            {
                return BadRequest();
            }

            var existing = await blogSpotService.GetBlogPostAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            try
            {
                await blogSpotService.Modify(User, blog);
            }
            catch (AuthorizationFailureException)
            {
                return Challenge();
            }

            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/BlogApi
        [HttpPost]
        public IActionResult PostBlog([FromBody] Models.Blog.BlogPost blog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // The BlogSpotService.Create() signature requires an
            // IFormFileCollection for file uploads. Reading
            // Request.Form.Files when the request is a plain JSON
            // body (e.g. from PostIt) throws
            // "This request does not have a Content-Type header.
            //  Forms are available from requests with bodies like
            //  POSTs and a form Content-Type of either
            //  application/x-www-form-urlencoded or
            //  multipart/form-data."
            //
            // Two valid use cases for this endpoint:
            //   1. JSON body only (no files) — PostIt path.
            //   2. multipart/form-data with a 'blog' field + 0..N
            //      files — future browser / server-rendered path.
            //
            // Branch on HasFormContentType: pass the form files when
            // present, pass an empty collection otherwise. The
            // FileSystem branch in BlogSpotService.Create then
            // short-circuits to "no files to handle".
            var files = Request.HasFormContentType
                ? Request.Form.Files
                : (IFormFileCollection)new FormFileCollection();
            var post = blogSpotService.Create(User.GetUserId(), blog, files);
            return CreatedAtRoute("GetBlog", new { id = post.Id }, post);
        }

        // DELETE: api/BlogApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var blog = await blogSpotService.GetBlogPostAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            await blogSpotService.Delete(User, id);
            return Ok(blog);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
