
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Yavsc.Models;
using Yavsc.ViewModels.Auth;
using Microsoft.AspNetCore.Mvc.Rendering;
using Yavsc.Models.Blog;
using Yavsc.Helpers;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Yavsc.ViewModels.Blog;
using System.Collections;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Yavsc.Controllers
{
    public class BlogspotController : Controller
    {
        readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        readonly RequestLocalizationOptions _localisationOptions;

        public BlogspotController(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IAuthorizationService authorizationService,
            IOptions<RequestLocalizationOptions> localisationOptions)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _authorizationService = authorizationService;
            _localisationOptions = localisationOptions.Value;
        }

        // GET: Blog
        [AllowAnonymous]
        public async Task<IActionResult> Index(string id)
        {
            if (!string.IsNullOrEmpty(id)) {
                return View("UserPosts", await UserPosts(id));
            }
            return View();
        }

        [Route("~/Title/{id?}")]
        [AllowAnonymous]
        public IActionResult Title(string id)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["Title"] = id;
            return View("Title", _context.Blogspot.Include(
                b => b.Author
            ).Where(x => x.Title == id && (x.Visible || x.AuthorId == uid )).OrderByDescending(
                x => x.DateCreated
            ).ToList());
        }

        private async Task<IEnumerable<BlogPost>> UserPosts(string userName, int pageLen=10, int pageNum=0)
        {
            string posterId = (await _context.Users.SingleOrDefaultAsync(u=>u.UserName == userName))?.Id ?? null ;
            return _context.UserPosts(posterId, User.Identity.Name);
        }
        // GET: Blog/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BlogPost blog = _context.Blogspot
            .Include(p => p.Author)
            .Include(p => p.Tags)
            .Include(p => p.Comments)
            .Include(p => p.ACL)
            .Single(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }
            if ( _authorizationService.AuthorizeAsync(User, blog, new ViewRequirement()).IsFaulted)
            {
                return new ChallengeResult();
            }
            foreach (var c in blog.Comments) {
                c.Author = _context.Users.First(u=>u.Id==c.AuthorId);
            }
            ViewData["apicmtctlr"] = "/api/blogcomments";
            ViewData["moderatoFlag"] = User.IsInRole(Constants.BlogModeratorGroupName);
            return View(blog);
        }
        void SetLangItems()
        {
            ViewBag.LangItems = _localisationOptions.SupportedUICultures.Select
            (
                sc => new SelectListItem { Value = sc.IetfLanguageTag, Text = sc.NativeName, Selected = System.Globalization.CultureInfo.CurrentUICulture == sc }
            );
        }

        // GET: Blog/Create
        [Authorize()]
        public IActionResult Create(string title)
        {
            var result = new BlogPostInputViewModel{Title=title,Content=""};
            ViewData["PostTarget"]="Create";
            SetLangItems();
            return View(result);
        }

        // POST: Blog/Create
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public IActionResult Create(BlogPostInputViewModel blogInput)
        {
            if (ModelState.IsValid)
            {
                BlogPost post = new BlogPost
                {
                    Title = blogInput.Title,
                    Content = blogInput.Content,
                    Photo = blogInput.Photo,
                    Rate = 0,
                    AuthorId = User.GetUserId()
                };
                _context.Blogspot.Add(post);
                _context.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("Unknown","Invalid Blog posted ...");
            ViewData["PostTarget"]="Create";
            return View("Edit",blogInput);
        }

        [Authorize()]
        // GET: Blog/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewData["PostTarget"]="Edit";
            BlogPost blog = _context.Blogspot.Include(x => x.Author).Include(x => x.ACL).Single(m => m.Id == id);

            if (blog == null)
            {
                return NotFound();
            }
            if (!_authorizationService.AuthorizeAsync(User, blog, new EditPermission()).IsFaulted)
            {
                ViewBag.ACL = _context.Circle.Where(
                c=>c.OwnerId == blog.AuthorId)
                .Select(
                    c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = blog.AuthorizeCircle(c.Id)
                    } 
                );
                SetLangItems();
                return View(blog);
            }
            else
            {
                return new ChallengeResult();
            }
        }

        // POST: Blog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken,Authorize()]
        public IActionResult Edit(BlogPost blog)
        {
            if (ModelState.IsValid)
            {
                var auth = _authorizationService.AuthorizeAsync(User, blog, new EditPermission());
                if (!auth.IsFaulted)
                {
                    // saves the change
                    _context.Update(blog);
                    _context.SaveChanges(User.GetUserId());
                    ViewData["StatusMessage"] = "Post modified";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewData["StatusMessage"] = "Accès restreint";
                    return new ChallengeResult();
                }
            }
            ViewData["PostTarget"]="Edit";
            return View(blog);
        }

        // GET: Blog/Delete/5
        [ActionName("Delete"),Authorize()]
        public IActionResult Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BlogPost blog = _context.Blogspot.Include(
               b => b.Author
           ).Single(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete"), Authorize()]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            BlogPost blog = _context.Blogspot.Single(m => m.Id == id && m.GetOwnerId()== User.GetUserId());
           
            _context.Blogspot.Remove(blog);
            _context.SaveChanges(User.GetUserId());
           
            return RedirectToAction("Index");
        }
    }
}
