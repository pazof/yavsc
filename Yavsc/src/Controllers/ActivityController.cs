using System.Linq;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    [ServiceFilter(typeof(LanguageActionFilter)),Authorize("AdministratorOnly")]
    public class ActivityController : Controller
    {
        private ApplicationDbContext _context;

        public ActivityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Activity
        public IActionResult Index()
        {
            return View(_context.Activities.ToList());
        }

        // GET: Activity/Details/5
        public IActionResult Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Activity activity = _context.Activities.Single(m => m.Code == id);
            if (activity == null)
            {
                return HttpNotFound();
            }

            return View(activity);
        }

        // GET: Activity/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Activity/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Activity activity)
        {
            if (ModelState.IsValid)
            {
                _context.Activities.Add(activity);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(activity);
        }

        // GET: Activity/Edit/5
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Activity activity = _context.Activities.Single(m => m.Code == id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            return View(activity);
        }

        // POST: Activity/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Activity activity)
        {
            if (ModelState.IsValid)
            {
                _context.Update(activity);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(activity);
        }

        // GET: Activity/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Activity activity = _context.Activities.Single(m => m.Code == id);
            if (activity == null)
            {
                return HttpNotFound();
            }

            return View(activity);
        }

        // POST: Activity/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            Activity activity = _context.Activities.Single(m => m.Code == id);
            _context.Activities.Remove(activity);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
