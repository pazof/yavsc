using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Yavsc.Models;
using System;
using Microsoft.AspNet.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Extensions.OptionsModel;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Yavsc.Controllers
{
    [ServiceFilter(typeof(LanguageActionFilter)),
    AllowAnonymous]
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
        public IActionResult Index(string id)
        {
            if (!string.IsNullOrEmpty(id))
               return UserPosts(id);
            return View(_context.Blogspot.Include(
               b=>b.Author
            ).Where(p=>p.visible));
        }

        [Route("/Title/{id?}")]
        public IActionResult Title(string id)
        {
           return View("Index", _context.Blogspot.Include(
               b=>b.Author
           ).Where(x=>x.title==id).ToList());
        }

        [Route("/Blog/{id?}")]
        public IActionResult UserPosts(string id)
        {

            if (User.IsSignedIn())
               return View("Index", _context.Blogspot.Include(
                b=>b.Author
                ).Where(x=>x.Author.UserName==id).ToList());
            return View("Index", _context.Blogspot.Include(
                b=>b.Author
                ).Where(x=>x.Author.UserName==id && x.visible).ToList());
        }
        // GET: Blog/Details/5
        public IActionResult Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Blog blog = _context.Blogspot.Include(
               b=>b.Author
           ).Single(m => m.Id == id);
            if (blog == null)
            {
                return HttpNotFound();
            }

            return View(blog);
        }

        // GET: Blog/Create
        public IActionResult Create()
        {
            return View( new Blog { AuthorId = User.GetUserId() } );
        }

        // POST: Blog/Create
        [HttpPost]
        [ValidateAntiForgeryToken,Authorize]
        public IActionResult Create(Blog blog)
        {
            blog.modified = blog.posted = DateTime.Now;
            blog.rate = 0;

            if (ModelState.IsValid)
            {
                blog.posted = DateTime.Now;
                _context.Blogspot.Add(blog);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            _logger.LogWarning("Invalid Blog entry ...");
            return View(blog);
        }

        // GET: Blog/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Blog blog = _context.Blogspot.Include(x=>x.Author).Single(m => m.Id == id);
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
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Blog blog)
        {
            if (ModelState.IsValid)
            {
                var auth = _authorizationService.AuthorizeAsync(User, blog, new EditRequirement());
                if (auth.Result)
                {
                    blog.modified = DateTime.Now;
                    _context.Update(blog);
                    _context.SaveChanges();
                    ViewData["StatusMessage"]="Post modified";
                    return RedirectToAction("Index");
                } // TODO Else hit me hard
                else {
                  ViewData["StatusMessage"]="Access denied ...";
                }
            }
            return View(blog);
        }

        // GET: Blog/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Blog blog = _context.Blogspot.Include(
               b=>b.Author
           ).Single(m => m.Id == id);
            if (blog == null)
            {
                return HttpNotFound();
            }

            return View(blog);
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete"), Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            Blog blog = _context.Blogspot.Single(m => m.Id == id);
            var auth = _authorizationService.AuthorizeAsync(User, blog, new EditRequirement());
            if (auth.Result) {
                _context.Blogspot.Remove(blog);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
