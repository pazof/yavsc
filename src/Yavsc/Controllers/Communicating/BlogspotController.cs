
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
        public async Task<IActionResult> Index(string id, int skip=0, int take=25)
        {
            if (!string.IsNullOrEmpty(id)) {
                return View("UserPosts", await UserPosts(id));
            }
            IEnumerable<BlogPost> posts;

            if (User.Identity.IsAuthenticated)
            {
                string viewerId = User.GetUserId();
                long[] usercircles = await _context.Circle.Include(c=>c.Members).
                    Where(c=>c.Members.Any(m=>m.MemberId == viewerId))
                    .Select(c=>c.Id).ToArrayAsync();
                
                posts = _context.BlogSpot
                    .Include(b => b.Author)
                    .Include(p=>p.ACL)
                    .Include(p=>p.Tags)
                    .Include(p=>p.Comments)
                    .Where(p =>(p.ACL.Count == 0) 
                    || (p.AuthorId == viewerId)
                    || (usercircles != null && p.ACL.Any(a => usercircles.Contains(a.CircleId)))
                    );
            }
            else 
            {
                 posts = _context.BlogSpot
                .Include(b => b.Author)
                .Include(p=>p.ACL)
                .Include(p=>p.Tags)
                .Include(p=>p.Comments)
                .Where(p => p.ACL.Count == 0 ).ToArray();
            }
          
            var data = posts.OrderByDescending( p=> p.DateCreated);
            var grouped = data.GroupBy(p=> p.Title).Skip(skip).Take(take);

            return View(grouped);
        }

        [Route("~/Title/{id?}")]
        [AllowAnonymous]
        public IActionResult Title(string id)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["Title"] = id;
            return View("Title", _context.BlogSpot.Include(
                b => b.Author
            ).Where(x => x.Title == id && (x.AuthorId == uid )).OrderByDescending(
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

            BlogPost blog = _context.BlogSpot
            .Include(p => p.Author)
            .Include(p => p.Tags)
            .Include(p => p.Comments)
            .Include(p => p.ACL)
            .Single(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }
            if ( _authorizationService.AuthorizeAsync(User, blog, new ReadPermission()).IsFaulted)
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
            var result = new BlogPostInputViewModel{Title=title
            };
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
                    AuthorId = User.GetUserId()
                };
                _context.BlogSpot.Add(post);
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
            BlogPost blog = _context.BlogSpot.Include(x => x.Author).Include(x => x.ACL).Single(m => m.Id == id);

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
                return View(new BlogPostEditViewModel
                {
                    Id = blog.Id,
                    Title = blog.Title,
                    Content = blog.Content,
                    ACL = blog.ACL,
                    Photo = blog.Photo
            });
            }
            else
            {
                return new ChallengeResult();
            }
        }

        // POST: Blog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken,Authorize()]
        public async Task<IActionResult> Edit(BlogPostEditViewModel blogEdit)
        {
            if (ModelState.IsValid)
            {
                var blog = _context.BlogSpot.SingleOrDefault(b=>b.Id == blogEdit.Id);
                if (blog == null) {
                    ModelState.AddModelError("Id", "not found");
                    return View();
                }
                if (!(await _authorizationService.AuthorizeAsync(User, blog, new EditPermission())).Succeeded) {
                    ViewData["StatusMessage"] = "Accès restreint";
                    return new ChallengeResult();
                }
                blog.Content=blogEdit.Content;
                blog.Title = blogEdit.Title;
                blog.Photo = blogEdit.Photo;
                blog.ACL = blogEdit.ACL;
                // saves the change
                _context.Update(blog);
                _context.SaveChanges(User.GetUserId());
                ViewData["StatusMessage"] = "Post modified";
                return RedirectToAction("Index");
            }
            ViewData["PostTarget"]="Edit";
            return View(blogEdit);
        }

        // GET: Blog/Delete/5
        [ActionName("Delete"),Authorize()]
        public IActionResult Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BlogPost blog = _context.BlogSpot.Include(
               b => b.Author
           ).Single(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete"), Authorize("IsTheAuthor")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            var uid = User.GetUserId();
            BlogPost blog = _context.BlogSpot.Single(m => m.Id == id);
           
            _context.BlogSpot.Remove(blog);
            _context.SaveChanges(User.GetUserId());
           
            return RedirectToAction("Index");
        }
    }
}
