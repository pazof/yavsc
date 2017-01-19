using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Relationship;

namespace Yavsc.Controllers
{
    public class CircleMemberController : Controller
    {
        private ApplicationDbContext _context;

        public CircleMemberController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: CircleMember
        public async Task<IActionResult> Index()
        {
            return View(await _context.CircleMembers.ToListAsync());
        }

        // GET: CircleMember/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            CircleMember circleMember = await _context.CircleMembers.SingleAsync(m => m.Id == id);
            if (circleMember == null)
            {
                return HttpNotFound();
            }

            return View(circleMember);
        }

        // GET: CircleMember/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CircleMember/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CircleMember circleMember)
        {
            if (ModelState.IsValid)
            {
                _context.CircleMembers.Add(circleMember);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(circleMember);
        }

        // GET: CircleMember/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            CircleMember circleMember = await _context.CircleMembers.SingleAsync(m => m.Id == id);
            if (circleMember == null)
            {
                return HttpNotFound();
            }
            return View(circleMember);
        }

        // POST: CircleMember/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CircleMember circleMember)
        {
            if (ModelState.IsValid)
            {
                _context.Update(circleMember);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(circleMember);
        }

        // GET: CircleMember/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            CircleMember circleMember = await _context.CircleMembers.SingleAsync(m => m.Id == id);
            if (circleMember == null)
            {
                return HttpNotFound();
            }

            return View(circleMember);
        }

        // POST: CircleMember/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            CircleMember circleMember = await _context.CircleMembers.SingleAsync(m => m.Id == id);
            _context.CircleMembers.Remove(circleMember);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
