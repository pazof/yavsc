using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Calendar;
using System;

namespace Yavsc.Controllers
{
    public class PeriodController : Controller
    {
        private ApplicationDbContext _context;

        public PeriodController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Period
        public async Task<IActionResult> Index()
        {
            return View(await _context.Period.ToListAsync());
        }

        // GET: Period/Details/5
        public async Task<IActionResult> Details(DateTime id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Period period = await _context.Period.SingleAsync(m => m.Start == id);
            if (period == null)
            {
                return HttpNotFound();
            }

            return View(period);
        }

        // GET: Period/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Period/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Period period)
        {
            if (ModelState.IsValid)
            {
                _context.Period.Add(period);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(period);
        }

        // GET: Period/Edit/5
        public async Task<IActionResult> Edit(DateTime id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Period period = await _context.Period.SingleAsync(m => m.Start == id);
            if (period == null)
            {
                return HttpNotFound();
            }
            return View(period);
        }

        // POST: Period/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Period period)
        {
            if (ModelState.IsValid)
            {
                _context.Update(period);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(period);
        }

        // GET: Period/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(DateTime id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Period period = await _context.Period.SingleAsync(m => m.Start == id);
            if (period == null)
            {
                return HttpNotFound();
            }

            return View(period);
        }

        // POST: Period/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(DateTime id)
        {
            Period period = await _context.Period.SingleAsync(m => m.Start == id);
            _context.Period.Remove(period);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
