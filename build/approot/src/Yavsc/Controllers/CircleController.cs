using System.Linq;
using Microsoft.AspNet.Mvc;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    [ServiceFilter(typeof(LanguageActionFilter))]
    public class CircleController : Controller
    {
        private ApplicationDbContext _context;

        public CircleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Circle
        public IActionResult Index()
        {
            return View(_context.CircleMembers.ToList());
        }

        // GET: Circle/Details/5
        public IActionResult Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            CircleMember circleMember = _context.CircleMembers.Single(m => m.Id == id);
            if (circleMember == null)
            {
                return HttpNotFound();
            }

            return View(circleMember);
        }

        // GET: Circle/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Circle/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CircleMember circleMember)
        {
            if (ModelState.IsValid)
            {
                _context.CircleMembers.Add(circleMember);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(circleMember);
        }

        // GET: Circle/Edit/5
        public IActionResult Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            CircleMember circleMember = _context.CircleMembers.Single(m => m.Id == id);
            if (circleMember == null)
            {
                return HttpNotFound();
            }
            return View(circleMember);
        }

        // POST: Circle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CircleMember circleMember)
        {
            if (ModelState.IsValid)
            {
                _context.Update(circleMember);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(circleMember);
        }

        // GET: Circle/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            CircleMember circleMember = _context.CircleMembers.Single(m => m.Id == id);
            if (circleMember == null)
            {
                return HttpNotFound();
            }

            return View(circleMember);
        }

        // POST: Circle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            CircleMember circleMember = _context.CircleMembers.Single(m => m.Id == id);
            _context.CircleMembers.Remove(circleMember);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
