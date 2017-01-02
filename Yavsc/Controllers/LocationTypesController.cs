using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Relationship;

namespace Yavsc.Controllers
{
    public class LocationTypesController : Controller
    {
        private ApplicationDbContext _context;

        public LocationTypesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: LocationTypes
        public IActionResult Index()
        {
            return View(_context.LocationType.ToList());
        }

        // GET: LocationTypes/Details/5
        public IActionResult Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            LocationType locationType = _context.LocationType.Single(m => m.Id == id);
            if (locationType == null)
            {
                return HttpNotFound();
            }

            return View(locationType);
        }

        // GET: LocationTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LocationTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LocationType locationType)
        {
            if (ModelState.IsValid)
            {
                _context.LocationType.Add(locationType);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(locationType);
        }

        // GET: LocationTypes/Edit/5
        public IActionResult Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            LocationType locationType = _context.LocationType.Single(m => m.Id == id);
            if (locationType == null)
            {
                return HttpNotFound();
            }
            return View(locationType);
        }

        // POST: LocationTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(LocationType locationType)
        {
            if (ModelState.IsValid)
            {
                _context.Update(locationType);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(locationType);
        }

        // GET: LocationTypes/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            LocationType locationType = _context.LocationType.Single(m => m.Id == id);
            if (locationType == null)
            {
                return HttpNotFound();
            }

            return View(locationType);
        }

        // POST: LocationTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            LocationType locationType = _context.LocationType.Single(m => m.Id == id);
            _context.LocationType.Remove(locationType);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
