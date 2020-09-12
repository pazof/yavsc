using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Streaming;

namespace Yavsc.Controllers
{
    public class LiveFlowController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LiveFlowController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: LiveFlow
        public async Task<IActionResult> Index()
        {
            var uid = User.GetUserId();
            var applicationDbContext = _context.LiveFlow.Where(f=>f.OwnerId == uid);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> AdminIndex()
        {
            var applicationDbContext = _context.LiveFlow.Include(l => l.Owner);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: LiveFlow/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            LiveFlow liveFlow = await _context.LiveFlow.SingleAsync(m => m.Id == id);
            if (liveFlow == null)
            {
                return HttpNotFound();
            }

            return View(liveFlow);
        }

        // GET: LiveFlow/Create
        public IActionResult Create()
        {
            ViewData["OwnerId"] = new SelectList(_context.ApplicationUser, "Id", "Owner");
            return View();
        }

        // POST: LiveFlow/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LiveFlow liveFlow)
        {
            if (ModelState.IsValid)
            {
                _context.LiveFlow.Add(liveFlow);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["OwnerId"] = new SelectList(_context.ApplicationUser, "Id", "Owner", liveFlow.OwnerId);
            return View(liveFlow);
        }

        // GET: LiveFlow/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            LiveFlow liveFlow = await _context.LiveFlow.SingleAsync(m => m.Id == id);
            if (liveFlow == null)
            {
                return HttpNotFound();
            }
            
            return View(liveFlow);
        }

        public async Task<IActionResult> AdminEdit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            LiveFlow liveFlow = await _context.LiveFlow.SingleAsync(m => m.Id == id);
            if (liveFlow == null)
            {
                return HttpNotFound();
            }
            
            ViewBag.OwnerId = _context.ApplicationUser.Select
            (u=> new SelectListItem(){Text=u.UserName,Value=u.Id,Selected=liveFlow.OwnerId==u.Id});
            return View("AdminEdit", liveFlow);
        }
        // POST: LiveFlow/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LiveFlow liveFlow)
        {
            if (User.GetUserId()!=liveFlow.OwnerId)
            {
                ModelState.AddModelError("OwnerId","denied");
            } 
            else if (ModelState.IsValid)
            {
                _context.Update(liveFlow);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(liveFlow);
        }


        // POST: LiveFlow/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("AdministratorOnly")]
        public async Task<IActionResult> AdminEdit(LiveFlow liveFlow)
        {
            if (ModelState.IsValid)
            {
                _context.Update(liveFlow);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["OwnerId"] = new SelectList(_context.ApplicationUser, "Id", "Owner", liveFlow.OwnerId);
            return View(liveFlow);
        }

        // GET: LiveFlow/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            LiveFlow liveFlow = await _context.LiveFlow.SingleAsync(m => m.Id == id);

            if (liveFlow == null)
            {
                return HttpNotFound();
            } 
            else if (User.GetUserId()!=liveFlow.OwnerId)
            {
                ModelState.AddModelError("OwnerId","denied");
            } 

            return View(liveFlow);
        }

        // POST: LiveFlow/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            LiveFlow liveFlow = await _context.LiveFlow.SingleAsync(m => m.Id == id);
            if (User.GetUserId()!=liveFlow.OwnerId)
            {
                ModelState.AddModelError("OwnerId","denied");
            } else 
            {
                _context.LiveFlow.Remove(liveFlow);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
