using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;

namespace Yavsc.Controllers
{
    using Models;
    using Models.IT.Maintaining;
    public class FeatureController : Controller
    {
        private ApplicationDbContext _context;

        public FeatureController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Feature
        public async Task<IActionResult> Index()
        {
            return View(await _context.Feature.ToListAsync());
        }

        // GET: Feature/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Feature feature = await _context.Feature.SingleAsync(m => m.Id == id);
            if (feature == null)
            {
                return HttpNotFound();
            }

            return View(feature);
        }

        // GET: Feature/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Feature/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Feature feature)
        {
            if (ModelState.IsValid)
            {
                _context.Feature.Add(feature);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(feature);
        }

        // GET: Feature/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Feature feature = await _context.Feature.SingleAsync(m => m.Id == id);
            if (feature == null)
            {
                return HttpNotFound();
            }
            var featureStatusEnumType = typeof(FeatureStatus);
            var fsstatuses = new List<SelectListItem>();
            foreach (var v in featureStatusEnumType.GetEnumValues())
             {
                 fsstatuses.Add(new SelectListItem { Value = v.ToString(), Text = featureStatusEnumType.GetEnumName(v) });
             }
            ViewBag.Statuses =  fsstatuses;
            return View(feature);
        }

        // POST: Feature/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Feature feature)
        {
            if (ModelState.IsValid)
            {
                _context.Update(feature);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(feature);
        }

        // GET: Feature/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Feature feature = await _context.Feature.SingleAsync(m => m.Id == id);
            if (feature == null)
            {
                return HttpNotFound();
            }

            return View(feature);
        }

        // POST: Feature/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            Feature feature = await _context.Feature.SingleAsync(m => m.Id == id);
            _context.Feature.Remove(feature);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
