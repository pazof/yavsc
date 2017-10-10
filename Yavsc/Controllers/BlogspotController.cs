
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
using Yavsc.ViewModels.Blogspot;
using Yavsc.Models.Blog;
// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Yavsc.Controllers
{
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
            if (!string.IsNullOrEmpty(id))
                return UserPosts(id);
            string uid = User.GetUserId();
            long[] usercircles = _context.Circle.Include(c=>c.Members).Where(c=>c.Members.Any(m=>m.MemberId == uid))
            .Select(c=>c.Id).ToArray();
            IQueryable<BlogPost> posts ;
            if (usercircles != null) {
                posts = _context.Blogspot.Include(b => b.Author)
                .Include(p=>p.Tags)
                .Include(p=>p.Comments)
                .Include(p=>p.ACL)
                .Where(p=> p.AuthorId == uid || p.Visible &&
                (p.ACL.Count == 0 || p.ACL.Any(a=> usercircles.Contains(a.CircleId))))
                            ;
            }
            else {
                posts = _context.Blogspot.Include(b => b.Author)
                .Include(p=>p.ACL).Where(p=>p.AuthorId == uid || p.Visible && p.ACL.Count == 0);
            }

            return View(posts.OrderByDescending( p=> p.DateModified)
            .GroupBy(p=> new BlogIndexKey { Title = p.Title, AuthorId = p.AuthorId } ).Skip(skip).Take(maxLen));
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

        [Route("/Blog/{id?}")]
        [AllowAnonymous]
        public IActionResult UserPosts(string id)
        {

            if (string.IsNullOrEmpty(id)) return Index(null);
            var uid = User.GetUserId();
            long[] usercircles = _context.Circle.Include(c=>c.Members).Where(c=>c.Members.Any(m=>m.MemberId == uid))
            .Select(c=>c.Id).ToArray();
            var result = (User.IsSignedIn())?
                 _context.Blogspot.Include(
                 b => b.Author
                 ).Include(p=>p.ACL).Where(x => x.Author.UserName == id && (x.Visible && (x.ACL.Count==0 || x.ACL.Any(a=> usercircles.Contains(a.CircleId))))):
             _context.Blogspot.Include(
                b => b.Author
                ).Where(x => x.Author.UserName == id && x.Visible);
                // BlogIndexKey
            return View("Index", result.OrderByDescending(p => p.DateCreated).ToList().GroupBy(p=>new BlogIndexKey { Title = p.Title, AuthorId = p.AuthorId } ));
        }
        // GET: Blog/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            BlogPost blog = _context.Blogspot.Include(
               b => b.Author
           )
           .Include(p=>p.Tags)
           .Include(p=>p.Comments)
           .Include(p => p.ACL).Single(m => m.Id == id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            if (!await _authorizationService.AuthorizeAsync(User, blog, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
            ViewData["apicmtctlr"] = "/api/blogcomments";
            ViewData["moderatoFlag"] = User.IsInRole(Constants.BlogModeratorGroupName);
            return View(blog);
        }

        // GET: Blog/Create
        [Authorize()]
        public IActionResult Create(string title)
        {
            var result = new BlogPost{Title=title};
            ViewData["PostTarget"]="Create";
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
