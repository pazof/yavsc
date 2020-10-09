using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Forms;

namespace Yavsc.Controllers
{
    public class FormsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FormsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Forms
        public async Task<IActionResult> Index()
        {
            return View(await _context.Form.ToListAsync());
        }

        // GET: Forms/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Form form = await _context.Form.SingleAsync(m => m.Id == id);
            if (form == null)
            {
                return HttpNotFound();
            }

            return View(form);
        }

        // GET: Forms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Forms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Form form)
        {
            if (ModelState.IsValid)
            {
                _context.Form.Add(form);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(form);
        }

        // GET: Forms/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Form form = await _context.Form.SingleAsync(m => m.Id == id);
            if (form == null)
            {
                return HttpNotFound();
            }
            return View(form);
        }

        // POST: Forms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Form form)
        {
            if (ModelState.IsValid)
            {
                _context.Update(form);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(form);
        }

        // GET: Forms/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Form form = await _context.Form.SingleAsync(m => m.Id == id);
            if (form == null)
            {
                return HttpNotFound();
            }

            return View(form);
        }

        // POST: Forms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            Form form = await _context.Form.SingleAsync(m => m.Id == id);
            _context.Form.Remove(form);
            await _context.SaveChangesAsync(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
