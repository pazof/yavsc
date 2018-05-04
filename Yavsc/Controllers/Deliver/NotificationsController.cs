using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Messaging;

namespace Yavsc.Controllers
{
    public class NotificationsController : Controller
    {
        private ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            return View(await _context.Notification.ToListAsync());
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Notification notification = await _context.Notification.SingleAsync(m => m.Id == id);
            if (notification == null)
            {
                return HttpNotFound();
            }

            return View(notification);
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Notifications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notification notification)
        {
            if (ModelState.IsValid)
            {
                _context.Notification.Add(notification);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(notification);
        }

        // GET: Notifications/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Notification notification = await _context.Notification.SingleAsync(m => m.Id == id);
            if (notification == null)
            {
                return HttpNotFound();
            }
            return View(notification);
        }

        // POST: Notifications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Notification notification)
        {
            if (ModelState.IsValid)
            {
                _context.Update(notification);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(notification);
        }

        // GET: Notifications/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Notification notification = await _context.Notification.SingleAsync(m => m.Id == id);
            if (notification == null)
            {
                return HttpNotFound();
            }

            return View(notification);
        }

        // POST: Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            Notification notification = await _context.Notification.SingleAsync(m => m.Id == id);
            _context.Notification.Remove(notification);
            await _context.SaveChangesAsync(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
