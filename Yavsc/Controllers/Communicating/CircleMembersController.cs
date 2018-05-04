using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Relationship;

namespace Yavsc.Controllers
{
    public class CircleMembersController : Controller
    {
        private ApplicationDbContext _context;

        public CircleMembersController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: CircleMembers
        public async Task<IActionResult> Index()
        {
            var uid = User.GetUserId();
            var applicationDbContext = _context.CircleMembers.Include(c => c.Circle).Include(c => c.Member)
            .Where(c=>c.Circle.OwnerId == uid);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: CircleMembers/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var uid = User.GetUserId();

            CircleMember circleMember = await _context.CircleMembers
            .Include(m=>m.Circle)
            .FirstOrDefaultAsync(c=>c.CircleId == id);
            if (circleMember == null)
            {
                return HttpNotFound();
            }

            return View(circleMember);
        }

        // GET: CircleMembers/Create
        public IActionResult Create()
        {
            var uid = User.GetUserId();
            ViewBag.CircleId = new SelectList(_context.Circle.Where(c=>c.OwnerId == uid), "Id", "Name");
            ViewBag.MemberId = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: CircleMembers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CircleMember circleMember)
        {
            var uid = User.GetUserId();
            var circle = _context.Circle.SingleOrDefault(c=>c.OwnerId == uid && c.Id == circleMember.CircleId);
            if (circle==null)
                return new BadRequestResult();

            if (ModelState.IsValid)
            {
                _context.CircleMembers.Add(circleMember);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            ViewData["CircleId"] = new SelectList(_context.Circle, "Id", "Name", circleMember.CircleId);
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "UserName", circleMember.MemberId);
            return View(circleMember);
        }

        // GET: CircleMembers/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var uid = User.GetUserId();
            CircleMember circleMember = await _context.CircleMembers
            .Include(m=>m.Member)
            .SingleOrDefaultAsync(m => m.CircleId == id && m.MemberId == uid);
            if (circleMember == null)
            {
                return HttpNotFound();
            }
            return View(circleMember);
        }

        // POST: CircleMembers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CircleMember circleMember)
        {
            if (ModelState.IsValid)
            {
                _context.Update(circleMember);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            ViewData["CircleId"] = new SelectList(_context.Circle, "Id", "Circle", circleMember.CircleId);
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Member", circleMember.MemberId);
            return View(circleMember);
        }

        // GET: CircleMembers/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var uid = User.GetUserId();

            CircleMember circleMember = await _context.CircleMembers
            .Include(m=>m.Circle)
            .Include(m=>m.Member)
            .SingleOrDefaultAsync(m => m.CircleId == id && m.MemberId == uid);
            if (circleMember == null)
            {
                return HttpNotFound();
            }

            return View(circleMember);
        }

        // POST: CircleMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            CircleMember circleMember = await _context.CircleMembers.SingleAsync(m => m.CircleId == id);
            _context.CircleMembers.Remove(circleMember);
            await _context.SaveChangesAsync(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
