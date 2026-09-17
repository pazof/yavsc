using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Yavsc.Blogspot;
using Yavsc.Server.Exceptions;
using Yavsc.Server.Helpers;
using static Yavsc.Constants;

namespace Yavsc.Blogs.Controllers
{
    [Authorize("BlogScope")]
    [Produces("application/json")]
    [Route(APIPrefix + "/" + BlogSpotPath)]
    public class BlogApiController : Controller
    {
        private readonly BlogSpotService blogSpotService;

        public BlogApiController(BlogSpotService blogSpotService)
        {
            this.blogSpotService = blogSpotService;
        }

        // GET: api/v1/blogspot
        [HttpGet]
        public async Task<IEnumerable<IBlogPost>> GetBlogspot(int start = 0, int take = 25)
        {
            return await blogSpotService.Index(User, null, start, take);
        }

        // GET: api/v1/blogspot/5
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

                return Ok(blog.GetPayload());
            }
            catch (AuthorizationFailureException)
            {
                return Challenge();
            }
        }

        // PUT: api/v1/blogspot/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBlog(long id)
        {
            var blog = await ReadBlogRequestAsync();
            if (blog is null)
            {
                return BadRequest(ModelState);
            }

            // Without [FromBody], the data-annotations attributes
            // ([Required] on Title, etc.) are no longer evaluated by
            // the model binder — the body is read manually. Run
            // validation explicitly so an empty Title still yields
            // 400 (and not a 500 from the DB NOT NULL constraint).
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(blog);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                    blog, validationContext, validationResults, validateAllProperties: true))
            {
                foreach (var result in validationResults)
                {
                    ModelState.AddModelError(
                        result.MemberNames.FirstOrDefault() ?? string.Empty,
                        result.ErrorMessage ?? "Invalid value.");
                }
                return BadRequest(ModelState);
            }

            // These properties are server-managed or optional graph members and
            // should not block JSON payloads coming from API clients.
            ModelState.Remove(nameof(Models.Blog.BlogPost.Author));
            ModelState.Remove(nameof(Models.Blog.BlogPost.Tags));
            ModelState.Remove(nameof(Models.Blog.BlogPost.Comments));
            ModelState.Remove(nameof(Models.Blog.BlogPost.UserCreated));
            ModelState.Remove(nameof(Models.Blog.BlogPost.UserModified));

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != blog.Id)
            {
                return BadRequest();
            }

            var files = Request.HasFormContentType
                ? Request.Form.Files
                : (IFormFileCollection)new FormFileCollection();

            var existing = await blogSpotService.GetBlogPostAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            try
            {
                await blogSpotService.Modify(User, blog);
                blogSpotService.AttachFiles(files, User.GetUserId(), id);
            }
            catch (AuthorizationFailureException)
            {
                return Challenge();
            }

            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/v1/blogspot
        [HttpPost]
        public async Task<IActionResult> PostBlog()
        {
            var blog = await ReadBlogRequestAsync();
            if (blog is null)
            {
                return BadRequest(ModelState);
            }

            // These properties are server-managed or optional graph members and
            // should not block JSON payloads coming from API clients.
            ModelState.Remove(nameof(Models.Blog.BlogPost.Author));
            ModelState.Remove(nameof(Models.Blog.BlogPost.Tags));
            ModelState.Remove(nameof(Models.Blog.BlogPost.Comments));
            ModelState.Remove(nameof(Models.Blog.BlogPost.UserCreated));
            ModelState.Remove(nameof(Models.Blog.BlogPost.UserModified));

            // Without [FromBody], the data-annotations attributes
            // ([Required] on Title, etc.) are no longer evaluated by
            // the model binder — the body is read manually. Run
            // validation explicitly so an empty Title still yields
            // 400 (and not a 500 from the DB NOT NULL constraint).
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(blog);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                    blog, validationContext, validationResults, validateAllProperties: true))
            {
                foreach (var result in validationResults)
                {
                    ModelState.AddModelError(
                        result.MemberNames.FirstOrDefault() ?? string.Empty,
                        result.ErrorMessage ?? "Invalid value.");
                }
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Two valid use cases for this endpoint:
            //   1. JSON body only (no files) — PostIt path.
            //   2. multipart/form-data with a 'blog' field + 0..N
            //      files — PostIt "create with attachments" path.
            //
            // Branch on HasFormContentType: pass the form files when
            // present, pass an empty collection otherwise. Reading
            // Request.Form.Files on a plain JSON request throws
            // "This request does not have a Content-Type header…",
            // and AttachFiles short-circuits on an empty collection.
            var files = Request.HasFormContentType
                ? Request.Form.Files
                : (IFormFileCollection)new FormFileCollection();
            var uid = User.GetUserId();
            var post = blogSpotService.Create(uid, blog);
            blogSpotService.AttachFiles(files, uid, post.Id);
            return CreatedAtRoute("GetBlog", new { id = post.Id },
            post.GetPayload());
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
            return Ok(blog.GetPayload());
        }

        /// <summary>
        /// Toggle a post's publication state. <c>true</c> adds
        /// a row to <c>blogSpotPublications</c> (the post
        /// becomes publicly readable via
        /// <c>PermissionHandler.IsPublic</c>); <c>false</c>
        /// removes it.
        ///
        /// <para>PUT (not POST) because the operation is
        /// idempotent — the resulting state is determined by
        /// the body, not by the request. Returns 204 No
        /// Content on success, 404 when the post does not
        /// exist, 403 (Challenge) when the caller is not the
        /// author.</para>
        /// </summary>
        // PUT: api/BlogApi/5/publish
        // body: { "publish": true }
        [HttpPut("{id}/publish")]
        public async Task<IActionResult> PutPublish(
            [FromRoute] long id,
            [FromBody] SetPublishBody body)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var ok = await blogSpotService.SetPublishAsync(User, id, body.Publish);
                if (!ok) return NotFound();
                return new StatusCodeResult(StatusCodes.Status204NoContent);
            }
            catch (AuthorizationFailureException)
            {
                return Challenge();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// Read a <see cref="Models.Blog.BlogPost"/> from either a
        /// JSON body or a multipart/form-data request carrying the
        /// payload in a <c>blog</c> form field. Shared by
        /// <see cref="PostBlog"/> and <see cref="PutBlog"/>: both
        /// accept the two shapes so PostIt can attach files on
        /// create as well as on update. Returns <c>null</c> (with
        /// the reason recorded in <c>ModelState</c>) when the
        /// multipart form has no usable <c>blog</c> field.
        /// </summary>
        private async Task<Models.Blog.BlogPost?> ReadBlogRequestAsync()
        {
            if (!Request.HasFormContentType)
            {
                return await Request.ReadFromJsonAsync<Models.Blog.BlogPost>();
            }

            var raw = Request.Form["blog"].ToString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                ModelState.AddModelError("blog", "A blog payload is required in the multipart form field 'blog'.");
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<Models.Blog.BlogPost>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
            }
            catch (JsonException ex)
            {
                ModelState.AddModelError("blog", $"Invalid blog JSON payload: {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Wire body for <c>PUT /api/BlogApi/{id}/publish</c>.
    /// Intentionally tiny: just the desired publication state.
    /// </summary>
    public sealed class SetPublishBody
    {
        public bool Publish { get; set; }
    }
}
