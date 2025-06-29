
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Yavsc.Models;
using Yavsc.ViewModels.Auth;
using Microsoft.AspNetCore.Mvc.Rendering;
using Yavsc.Models.Blog;
using Yavsc.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Yavsc.ViewModels.Blog;
using Yavsc.Server.Exceptions;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Yavsc.Controllers
{
    public class BlogspotController : Controller
    {
        readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        readonly RequestLocalizationOptions _localisationOptions;

        readonly BlogSpotService blogSpotService;
        public BlogspotController(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IAuthorizationService authorizationService,
            IOptions<RequestLocalizationOptions> localisationOptions,
            BlogSpotService blogSpotService)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _authorizationService = authorizationService;
            _localisationOptions = localisationOptions.Value;
            this.blogSpotService = blogSpotService;
        }

        // GET: Blog
        [AllowAnonymous]
        public async Task<IActionResult> Index(string id, int skip = 0, int take = 25)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return View("UserPosts",
                await blogSpotService.UserPosts(id, User.GetUserId(),
                skip, take));
            }
            var byTitle = await this.blogSpotService.IndexByTitle(User, id, skip, take);
            return View(byTitle);
        }

        [Route("~/Title/{id?}")]
        [AllowAnonymous]
        public IActionResult Title(string id)
        {
            ViewData["Title"] = id;
            return View("Title", blogSpotService.ByTitle(id));
        }

        private async Task<IEnumerable<BlogPost>> UserPosts(string userName, int pageLen = 10, int pageNum = 0)
        {
            return await blogSpotService.UserPosts(userName, User.GetUserId(), pageLen, pageNum);

        }
        // GET: Blog/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return this.NotFound();

            var blog = await blogSpotService.Details(User, id.Value);
            ViewData["apicmtctlr"] = "/api/blogcomments";
            ViewData["moderatoFlag"] = User.IsInRole(Constants.BlogModeratorGroupName);

            return View(blog);
        }
        void SetLangItems()
        {
            ViewBag.LangItems = _localisationOptions.SupportedUICultures?.Select
            (
                sc => new SelectListItem { Value = sc.IetfLanguageTag, Text = sc.NativeName, Selected = System.Globalization.CultureInfo.CurrentUICulture == sc }
            );
        }

        // GET: Blog/Create
        [Authorize()]
        public IActionResult Create(string title)
        {
            var result = new BlogPostInputViewModel
            {
                Title = title
            };
            SetLangItems();
            return View(result);
        }

        // POST: Blog/Create
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public IActionResult Create(BlogPostInputViewModel blogInput)
        {
            if (ModelState.IsValid)
            {
                BlogPost post = blogSpotService.Create(User.GetUserId(),
                blogInput);
                return RedirectToAction("Index");
            }
            return View("Edit", blogInput);
        }

        [Authorize()]
        // GET: Blog/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            try
            {
                BlogPost blog = await blogSpotService.GetPostForEdition(User, id.Value);
                if (blog == null)
                {
                    return NotFound();
                }
                SetLangItems();
                return View(new BlogPostEditViewModel
                {
                    Id = blog.Id,
                    Title = blog.Title,
                    Content = blog.Content,
                    ACL = blog.ACL,
                    Photo = blog.Photo
                });

            }
            catch (AuthorizationFailureException)
            {
                return new ChallengeResult();
            }
        }

        // POST: Blog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken, Authorize()]
        public async Task<IActionResult> Edit(BlogPostEditViewModel blogEdit)
        {
            if (ModelState.IsValid)
            {
                await blogSpotService.Modify(User, blogEdit);
                ViewData["StatusMessage"] = "Post modified";
                return RedirectToAction("Index");
            }
            return View(blogEdit);
        }

        // GET: Blog/Delete/5
        [ActionName("Delete"), Authorize()]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BlogPost blog = await blogSpotService.GetPostForEdition(User, id.Value);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete"), Authorize("IsTheAuthor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            await blogSpotService.Delete(User, id);
            return RedirectToAction("Index");
        }
    }
}
