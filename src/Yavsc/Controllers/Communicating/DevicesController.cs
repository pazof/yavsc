using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;


namespace Yavsc.Controllers
{
    using Models;
    using Models.Identity;
    public class DevicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DevicesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: GCMDevices
        public async Task<IActionResult> Index()
        {
            var uid = User.GetUserId();

            var applicationDbContext = _context.DeviceDeclaration.Include(g => g.DeviceOwner).Where(d=>d.DeviceOwnerId == uid);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: GCMDevices/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            DeviceDeclaration googleCloudMobileDeclaration = await _context.DeviceDeclaration.SingleAsync(m => m.DeviceId == id);
            if (googleCloudMobileDeclaration == null)
            {
                return HttpNotFound();
            }

            return View(googleCloudMobileDeclaration);
        }

        // GET: GCMDevices/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            DeviceDeclaration googleCloudMobileDeclaration = await _context.DeviceDeclaration.SingleAsync(m => m.DeviceId == id);
            if (googleCloudMobileDeclaration == null)
            {
                return HttpNotFound();
            }

            return View(googleCloudMobileDeclaration);
        }

        // POST: GCMDevices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            DeviceDeclaration googleCloudMobileDeclaration = await _context.DeviceDeclaration.SingleAsync(m => m.DeviceId == id);
            _context.DeviceDeclaration.Remove(googleCloudMobileDeclaration);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
