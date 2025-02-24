using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Yavsc.Controllers
{
    using Microsoft.Extensions.Logging;
    using Models;
    using Models.Workflow;
    using Yavsc.ViewModels.Workflow;
    using Yavsc.Services;
    using System.Threading.Tasks;
    using Yavsc.Helpers;
    using Microsoft.EntityFrameworkCore;

    [Authorize]
    public class DoController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        readonly ILogger logger;
        readonly IBillingService billing;
        public DoController(
            ApplicationDbContext context,
            IBillingService billing,
            ILogger<DoController> logger)
        {
            dbContext = context;
            this.billing = billing;
            this.logger = logger;
        }

        // GET: /Do/Index
        [HttpGet]
        public IActionResult Index(string id)
        {
            if (id == null)
                id = User.GetUserId();

            var userActivities = dbContext.UserActivities.Include(u => u.Does)
            .Include(u => u.User).Where(u=> u.UserId == id)
            .OrderByDescending(u => u.Weight);
            return View(userActivities.ToList());
        }

        // GET: Do/Details/5
        public async Task<IActionResult> Details(string id, string activityCode)
        {

            if (id == null || activityCode == null)
            {
                return NotFound();
            }

            UserActivity userActivity = dbContext.UserActivities.Include(m=>m.Does)
            .Include(m=>m.User).Single(m => m.DoesCode == activityCode && m.UserId == id);
            if (userActivity == null)
            {
                return NotFound();
            }
            bool hasConfigurableSettings = (userActivity.Does.SettingsClassName != null);
            var settings = await billing.GetPerformersSettingsAsync(activityCode, id);
            ViewBag.ProfileType = Config.ProfileTypes.Single(t=>t.FullName==userActivity.Does.SettingsClassName);
             
            var gift = new UserActivityViewModel {
                    Declaration = userActivity,
                    Settings = settings,
                    NeedsSettings =  hasConfigurableSettings
                };
            return View (gift);
        }

        // GET: Do/Create
        [ActionName("Create"),Authorize]
        public IActionResult Create(string userId)
        {
            if (userId==null)
                userId = User.GetUserId();
            var model = new UserActivity { UserId = userId };
            ViewBag.DoesCode = new SelectList(dbContext.Activities, "Code", "Name");
            //ViewData["UserId"] = userId;
            ViewBag.UserId = new SelectList(dbContext.Performers.Include(p=>p.Performer), "PerformerId", "Performer", userId);
            return View(model);
        }

        // POST: Do/Create
        [HttpPost(),ActionName("Create"),Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserActivity userActivity)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Administrator"))
               if (uid != userActivity.UserId)
                    ModelState.AddModelError("User","You're not admin.");
            if (userActivity.UserId == null) userActivity.UserId = uid;
            if (ModelState.IsValid)
            {
                dbContext.UserActivities.Add(userActivity);
                dbContext.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            ViewBag.DoesCode = new SelectList(dbContext.Activities, "Code", "Name", userActivity.DoesCode);
            ViewBag.UserId = new SelectList(dbContext.Performers.Include(p=>p.Performer), "PerformerId", "User", userActivity.UserId);
            return View(userActivity);
        }

        // GET: Do/Edit/5
        [Authorize]
        public IActionResult Edit(string id, string activityCode)
        {
            if (id == null)
            {
                return NotFound();
            }

            UserActivity userActivity = dbContext.UserActivities.Include(
                u=>u.Does
            ).Include(
                u=>u.User
             ).Single(m => m.DoesCode == activityCode && m.UserId == id);
            if (userActivity == null)
            {
                return NotFound();
            }
            ViewData["DoesCode"] = new SelectList(dbContext.Activities, "Code", "Does", userActivity.DoesCode);
            ViewData["UserId"] = new SelectList(dbContext.Performers, "PerformerId", "User", userActivity.UserId);
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
                dbContext.Update(userActivity);
                dbContext.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            ViewData["DoesCode"] = new SelectList(dbContext.Activities, "Code", "Does", userActivity.DoesCode);
            ViewData["UserId"] = new SelectList(dbContext.Performers, "PerformerId", "User", userActivity.UserId);
            return View(userActivity);
        }

        // GET: Do/Delete/5
        [ActionName("Delete"),Authorize]
        public IActionResult Delete(string id, string activityCode)
        {
            if (id == null)
            {
                return NotFound();
            }

            UserActivity userActivity = dbContext.UserActivities.Single(m => m.UserId == id && m.DoesCode == activityCode);

            if (userActivity == null)
            {
                return NotFound();
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
            dbContext.UserActivities.Remove(userActivity);
            dbContext.SaveChanges(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
