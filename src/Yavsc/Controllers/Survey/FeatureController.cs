using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Yavsc.Controllers
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;
    using Models;
    using Models.IT.Evolution;
    using Yavsc.Server.Helpers;

    public class FeatureController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<BugController> _bugLocalizer;

        IEnumerable<SelectListItem> Statuses(FeatureStatus ?status) =>
            _bugLocalizer.CreateSelectListItems(typeof(FeatureStatus), status);
        public FeatureController(ApplicationDbContext context, IStringLocalizer<BugController> bugLocalizer)
        {
            _context = context;    
            _bugLocalizer = bugLocalizer;
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
                return NotFound();
            }

            Feature feature = await _context.Feature.SingleAsync(m => m.Id == id);
            if (feature == null)
            {
                return NotFound();
            }

            return View(feature);
        }

        // GET: Feature/Create
        public IActionResult Create()
        {
            ViewBag.FeatureStatus = Statuses(default(FeatureStatus));
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
            ViewBag.FeatureStatus = Statuses(default(FeatureStatus));
            return View(feature);
        }

        // GET: Feature/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Feature feature = await _context.Feature.SingleAsync(m => m.Id == id);
            if (feature == null)
            {
                return NotFound();
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
                return NotFound();
            }

            Feature feature = await _context.Feature.SingleAsync(m => m.Id == id);
            if (feature == null)
            {
                return NotFound();
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
