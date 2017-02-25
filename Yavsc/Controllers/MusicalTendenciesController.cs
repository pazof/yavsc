using System.Linq;
using Microsoft.AspNet.Mvc;

namespace Yavsc.Controllers
{
    using System.Security.Claims;
    using Models;
    using Models.Musical;
    public class MusicalTendenciesController : Controller
    {
        private ApplicationDbContext _context;

        public MusicalTendenciesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: MusicalTendencies
        public IActionResult Index()
        {
            return View(_context.MusicalTendency.ToList());
        }

        // GET: MusicalTendencies/Details/5
        public IActionResult Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            MusicalTendency musicalTendency = _context.MusicalTendency.Single(m => m.Id == id);
            if (musicalTendency == null)
            {
                return HttpNotFound();
            }

            return View(musicalTendency);
        }

        // GET: MusicalTendencies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MusicalTendencies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MusicalTendency musicalTendency)
        {
            if (ModelState.IsValid)
            {
                _context.MusicalTendency.Add(musicalTendency);
                _context.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(musicalTendency);
        }

        // GET: MusicalTendencies/Edit/5
        public IActionResult Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            MusicalTendency musicalTendency = _context.MusicalTendency.Single(m => m.Id == id);
            if (musicalTendency == null)
            {
                return HttpNotFound();
            }
            return View(musicalTendency);
        }

        // POST: MusicalTendencies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MusicalTendency musicalTendency)
        {
            if (ModelState.IsValid)
            {
                _context.Update(musicalTendency);
                _context.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(musicalTendency);
        }

        // GET: MusicalTendencies/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            MusicalTendency musicalTendency = _context.MusicalTendency.Single(m => m.Id == id);
            if (musicalTendency == null)
            {
                return HttpNotFound();
            }

            return View(musicalTendency);
        }

        // POST: MusicalTendencies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            MusicalTendency musicalTendency = _context.MusicalTendency.Single(m => m.Id == id);
            _context.MusicalTendency.Remove(musicalTendency);
            _context.SaveChanges(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
