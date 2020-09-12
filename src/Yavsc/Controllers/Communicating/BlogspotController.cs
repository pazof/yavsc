
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Authorization;
using Microsoft.Data.Entity;
using Microsoft.Extensions.OptionsModel;
using Yavsc.Models;
using Yavsc.ViewModels.Auth;
using Microsoft.AspNet.Mvc.Rendering;
using Yavsc.Models.Blog;
using Yavsc.Helpers;
using Microsoft.AspNet.Localization;

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
        public async Task<IActionResult> Index(string id, int skip=0, int maxLen=25)
        {
            if (!string.IsNullOrEmpty(id)) {
                return await UserPosts(id);
            }
            return View();
        }

        [Route("/Title/{id?}")]
        [AllowAnonymous]
        public IActionResult Title(string id)
        {
            var uid = User.GetUserId();
            ViewData["Title"] = id;
            return View("Title", _context.Blogspot.Include(
                b => b.Author
            ).Where(x => x.Title == id && (x.Visible || x.AuthorId == uid )).OrderByDescending(
                x => x.DateCreated
            ).ToList());
        }

        [Route("/Blog/{userName}/{pageLen?}/{pageNum?}")]
        [AllowAnonymous]
        public async Task<IActionResult> UserPosts(string userName, int pageLen=10, int pageNum=0)
        {
            string posterId = (await _context.Users.SingleOrDefaultAsync(u=>u.UserName == userName))?.Id ?? null ;
            var result = _context.UserPosts(posterId, User.Identity.Name);
            return View("Index", result.OrderByDescending(p => p.DateCreated).ToList().Skip(pageLen*pageNum).Take(pageLen).GroupBy(p=> p.Title ));
        }
        // GET: Blog/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            BlogPost blog = _context.Blogspot
            .Include(p => p.Author)
            .Include(p => p.Tags)
            .Include(p => p.Comments)
            .Include(p => p.ACL)
            .Single(m => m.Id == id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            if (!await _authorizationService.AuthorizeAsync(User, blog, new ViewRequirement()))
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
            var result = new BlogPost{Title=title};
            ViewData["PostTarget"]="Create";
            SetLangItems();
            return View("Edit",result);
        }

        // POST: Blog/Create
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public IActionResult Create(Models.Blog.BlogPost blog)
        {
            blog.Rate = 0;
            blog.AuthorId = User.GetUserId();
            blog.Id=0;
            if (ModelState.IsValid)
            {

                _context.Blogspot.Add(blog);
                _context.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("Unknown","Invalid Blog posted ...");
            ViewData["PostTarget"]="Create";
            return View("Edit",blog);
        }
        [Authorize()]
        // GET: Blog/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            ViewData["PostTarget"]="Edit";
            BlogPost blog = _context.Blogspot.Include(x => x.Author).Include(x => x.ACL).Single(m => m.Id == id);


            if (blog == null)
            {
                return HttpNotFound();
            }
            if (await _authorizationService.AuthorizeAsync(User, blog, new EditRequirement()))
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
                var auth = _authorizationService.AuthorizeAsync(User, blog, new EditRequirement());
                if (auth.Result)
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
                return HttpNotFound();
            }

            BlogPost blog = _context.Blogspot.Include(
               b => b.Author
           ).Single(m => m.Id == id);
            if (blog == null)
            {
                return HttpNotFound();
            }

            return View(blog);
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete"), Authorize()]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            BlogPost blog = _context.Blogspot.Single(m => m.Id == id);
            var auth = _authorizationService.AuthorizeAsync(User, blog, new EditRequirement());
            if (auth.Result)
            {
                _context.Blogspot.Remove(blog);
                _context.SaveChanges(User.GetUserId());
            }
            return RedirectToAction("Index");
        }
    }
}
