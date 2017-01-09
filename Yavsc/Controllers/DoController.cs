using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Workflow;

namespace Yavsc.Controllers
{
    [Authorize]
    public class DoController : Controller
    {
        private ApplicationDbContext _context;

        public DoController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Do
        [HttpGet,ActionName("Index")]
        public IActionResult Index(string id)
        {
            if (id == null)
                id = User.GetUserId();

            var applicationDbContext = _context.UserActivities.Include(u => u.Does).Include(u => u.User).Where(u=> u.UserId == id);
            return View(applicationDbContext.ToList());
        }

        // GET: Do/Details/5
        public IActionResult Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            UserActivity userActivity = _context.UserActivities.Single(m => m.Id == id);
            if (userActivity == null)
            {
                return HttpNotFound();
            }

            return View(userActivity);
        }

        // GET: Do/Create
        [ActionName("Create"),Authorize]
        public IActionResult Create(string userId)
        {
            if (userId==null)
                userId = User.GetUserId();
            ViewBag.DoesCode = new SelectList(_context.Activities, "Code", "Name");
            //ViewData["UserId"] = userId;
            ViewBag.UserId = new SelectList(_context.Performers.Include(p=>p.Performer), "PerformerId", "Performer", userId);
            return View();
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
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DoesCode = new SelectList(_context.Activities, "Code", "Name", userActivity.DoesCode);
            ViewBag.UserId = new SelectList(_context.Performers.Include(p=>p.Performer), "PerformerId", "User", userActivity.UserId);
            return View(userActivity);
        }

        // GET: Do/Edit/5
        [Authorize]
        public IActionResult Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            UserActivity userActivity = _context.UserActivities.Single(m => m.Id == id);
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
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewData["DoesCode"] = new SelectList(_context.Activities, "Code", "Does", userActivity.DoesCode);
            ViewData["UserId"] = new SelectList(_context.Performers, "PerformerId", "User", userActivity.UserId);
            return View(userActivity);
        }

        // GET: Do/Delete/5
        [ActionName("Delete"),Authorize]
        public IActionResult Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            UserActivity userActivity = _context.UserActivities.Single(m => m.Id == id);

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
        public IActionResult DeleteConfirmed(long id)
        {
            UserActivity userActivity = _context.UserActivities.Single(m => m.Id == id);
            if (!User.IsInRole("Administrator"))
               if (User.GetUserId() != userActivity.UserId) {
                    ModelState.AddModelError("User","You're not admin.");
                    return RedirectToAction("Index");
               }
            _context.UserActivities.Remove(userActivity);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
