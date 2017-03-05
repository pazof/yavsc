using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;

namespace Yavsc.Controllers
{
    using Microsoft.Extensions.Logging;
    using Models;
    using Models.Workflow;
    using Yavsc.Exceptions;
    using Yavsc.ViewModels.Workflow;

    [Authorize]
    public class DoController : Controller
    {
        private ApplicationDbContext _context;
        ILogger _logger;

        public DoController(ApplicationDbContext context,ILogger<DoController> logger)
        {
            _context = context;    
            _logger = logger;
        }

        // GET: /Do/Index
        [HttpGet]
        public IActionResult Index(string id)
        {
            if (id == null)
                id = User.GetUserId();

            var userActivities = _context.UserActivities.Include(u => u.Does)
            .Include(u => u.User).Where(u=> u.UserId == id)
            .OrderByDescending(u => u.Weight);
            return View(userActivities.ToList());
        }

        // GET: Do/Details/5
        public IActionResult Details(string id, string activityCode)
        {
            
            if (id == null || activityCode == null)
            {
                return HttpNotFound();
            }

            UserActivity userActivity = _context.UserActivities.Include(m=>m.Does)
            .Include(m=>m.User).Single(m => m.DoesCode == activityCode && m.UserId == id);
            if (userActivity == null)
            {
                return HttpNotFound();
            }
            
            bool hasConfigurableSettings = (userActivity.Does.SettingsClassName != null);
            if (hasConfigurableSettings) {

                ViewBag.ProfileType = Startup.ProfileTypes.Single(t=>t.FullName==userActivity.Does.SettingsClassName);
            

                var dbset = _context.GetDbSet(userActivity.Does.SettingsClassName);
                if (dbset == null) throw new InvalidWorkflowModelException($"pas de db set pour {userActivity.Does.SettingsClassName}, vous avez peut-être besoin de décorer votre propriété avec l'attribut [ActivitySettings]");
                return View(new UserActivityViewModel { 
                Declaration = userActivity, 
                HasSettings = dbset?.Any(ua=>ua.UserId==id) ?? false,
                NeedsSettings =  hasConfigurableSettings
                } );
            }
            return View(new UserActivityViewModel { 
                Declaration = userActivity, 
                HasSettings = false,
                NeedsSettings =  hasConfigurableSettings
                } );
        }

        // GET: Do/Create
        [ActionName("Create"),Authorize]
        public IActionResult Create(string userId)
        {
            if (userId==null)
                userId = User.GetUserId();
            var model = new UserActivity { UserId = userId };
            ViewBag.DoesCode = new SelectList(_context.Activities, "Code", "Name");
            //ViewData["UserId"] = userId;
            ViewBag.UserId = new SelectList(_context.Performers.Include(p=>p.Performer), "PerformerId", "Performer", userId);
            return View(model);
        }

        // POST: Do/Create
        [HttpPost(),ActionName("Create"),Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserActivity userActivity)
        {
            var uid = User.GetUserId();
            if (!User.IsInRole("Administrator"))
               if (uid != userActivity.UserId)
                    ModelState.AddModelError("User","You're not admin.");
            if (userActivity.UserId == null) userActivity.UserId = uid;
            if (ModelState.IsValid)
            {
                _context.UserActivities.Add(userActivity);
                _context.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            ViewBag.DoesCode = new SelectList(_context.Activities, "Code", "Name", userActivity.DoesCode);
            ViewBag.UserId = new SelectList(_context.Performers.Include(p=>p.Performer), "PerformerId", "User", userActivity.UserId);
            return View(userActivity);
        }

        // GET: Do/Edit/5
        [Authorize]
        public IActionResult Edit(string id, string activityCode)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            UserActivity userActivity = _context.UserActivities.Include(
                u=>u.Does
            ).Include(
                u=>u.User
             ).Single(m => m.DoesCode == activityCode && m.UserId == id);
            if (userActivity == null)
            {
                return HttpNotFound();
            }
            ViewData["DoesCode"] = new SelectList(_context.Activities, "Code", "Does", userActivity.DoesCode);
            ViewData["UserId"] = new SelectList(_context.Performers, "PerformerId", "User", userActivity.UserId);
            return View(userActivity);
        }

        // POST: Do/Edit/5
        [HttpPost,Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UserActivity userActivity)
        {
            if (!User.IsInRole("Administrator"))
               if (User.GetUserId() != userActivity.UserId)
                    ModelState.AddModelError("User","You're not admin.");
            if (ModelState.IsValid)
            {
                _context.Update(userActivity);
                _context.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            ViewData["DoesCode"] = new SelectList(_context.Activities, "Code", "Does", userActivity.DoesCode);
            ViewData["UserId"] = new SelectList(_context.Performers, "PerformerId", "User", userActivity.UserId);
            return View(userActivity);
        }

        // GET: Do/Delete/5
        [ActionName("Delete"),Authorize]
        public IActionResult Delete(string id, string activityCode)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            UserActivity userActivity = _context.UserActivities.Single(m => m.UserId == id && m.DoesCode == activityCode);

            if (userActivity == null)
            {
                return HttpNotFound();
            }
            if (!User.IsInRole("Administrator"))
               if (User.GetUserId() != userActivity.UserId)
                    ModelState.AddModelError("User","You're not admin.");
            return View(userActivity);
        }

        // POST: Do/Delete/5
        [HttpPost, ActionName("Delete"),Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(UserActivity userActivity)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            if (!User.IsInRole("Administrator"))
               if (User.GetUserId() != userActivity.UserId) {
                    ModelState.AddModelError("User","You're not admin.");
                    return RedirectToAction("Index");
               }
            _context.UserActivities.Remove(userActivity);
            _context.SaveChanges(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
