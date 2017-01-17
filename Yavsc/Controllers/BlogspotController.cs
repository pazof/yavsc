
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

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Yavsc.Controllers
{
    [ServiceFilter(typeof(LanguageActionFilter))]
    public class BlogspotController : Controller
    {
        ILogger _logger;
        private ApplicationDbContext _context;

        private SiteSettings _siteSettings;
        private IAuthorizationService _authorizationService;
        public BlogspotController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILoggerFactory loggerFactory,
            IAuthorizationService authorizationService,
            IOptions<SiteSettings> siteSettings)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _authorizationService = authorizationService;
            _siteSettings = siteSettings.Value;
        }

        // GET: Blog
        [AllowAnonymous]
        public IActionResult Index(string id, int skip=0, int maxLen=25)
        {
            string uid = null;
            if (User.IsSignedIn())
                uid = User.GetUserId();
            if (!string.IsNullOrEmpty(id))
                return UserPosts(id);
            return View(_context.Blogspot.Include(
               b => b.Author
            ).Where(p => p.Visible || p.AuthorId == uid ).OrderByDescending(p => p.DateCreated)
            .Skip(skip).Take(maxLen));
        }

        [Route("/Title/{id?}")]
        [AllowAnonymous]
        public IActionResult Title(string id)
        {
            var uid = User.GetUserId();
            return View("Index", _context.Blogspot.Include(
                b => b.Author
            ).Where(x => x.Title == id && (x.Visible || x.AuthorId == uid )).ToList());
        }

        [Route("/Blog/{id?}")]
        [AllowAnonymous]
        public IActionResult UserPosts(string id)
        {
            if (string.IsNullOrEmpty(id))
            return View("Index",_context.Blogspot.Include(
               b => b.Author
            ).Where(p => p.Visible));
            if (User.IsSignedIn())
                return View("Index", _context.Blogspot.Include(
                 b => b.Author
                 ).Where(x => x.Author.UserName == id).ToList());
            return View("Index", _context.Blogspot.Include(
                b => b.Author
                ).Where(x => x.Author.UserName == id && x.Visible).ToList());
        }
        // GET: Blog/Details/5
        [AllowAnonymous]
        public IActionResult Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Blog blog = _context.Blogspot.Include(
               b => b.Author
           ).Single(m => m.Id == id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            
            return View(blog);
        }

        // GET: Blog/Create
        [Authorize()]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Blog/Create
        [HttpPost, Authorize(), ValidateAntiForgeryToken]
        public IActionResult Create(Blog blog)
        {
            blog.Rate = 0;
            blog.AuthorId = User.GetUserId();
            ModelState.ClearValidationState("AuthorId");
            if (ModelState.IsValid)
            {
                _context.Blogspot.Add(blog);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("Unknown","Invalid Blog posted ...");
            return View(blog);
        }
        [Authorize()]
        // GET: Blog/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Blog blog = _context.Blogspot.Include(x => x.Author).Single(m => m.Id == id);


            if (blog == null)
            {
                return HttpNotFound();
            }
            if (await _authorizationService.AuthorizeAsync(User, blog, new EditRequirement()))
            {
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
        public IActionResult Edit(Blog blog)
        {
            if (ModelState.IsValid)
            {
                var auth = _authorizationService.AuthorizeAsync(User, blog, new EditRequirement());
                if (auth.Result)
                {
                    _context.Update(blog);
                    _context.SaveChanges();
                    ViewData["StatusMessage"] = "Post modified";
                    return RedirectToAction("Index");
                } // TODO Else hit me hard
                else
                {
                    ViewData["StatusMessage"] = "Access denied ...";
                }
            }
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

            Blog blog = _context.Blogspot.Include(
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
            Blog blog = _context.Blogspot.Single(m => m.Id == id);
            var auth = _authorizationService.AuthorizeAsync(User, blog, new EditRequirement());
            if (auth.Result)
            {
                _context.Blogspot.Remove(blog);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
