using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Yavsc.Models;
using Yavsc.Server.Models.Access;

namespace Yavsc.Controllers
{
    [Authorize()]
    public class MyFSRulesController : Controller
    {
        private ApplicationDbContext _context;
        private ILogger _logger;

        public MyFSRulesController(ApplicationDbContext context,
         ILoggerFactory loggerFactory)
        {
            _context = context;    
            _logger = loggerFactory.CreateLogger<MyFSRulesController>();
        }

        // GET: MyFSRules
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.CircleAuthorizationToFile.Include(c => c.Circle)
            .Where (m=>m.Circle.OwnerId == User.GetUserId());
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: MyFSRules/Details/5
        public async Task<IActionResult> Details(long circleId, string fullPath)
        {

            var uid = User.GetUserId();
            _logger.LogInformation($"Searching fsa for {uid} :\n {circleId}/{fullPath}");
            CircleAuthorizationToFile circleAuthorizationToFile =
             await _context.CircleAuthorizationToFile
             .Include(m=>m.Circle)
             .SingleOrDefaultAsync(m => ((m.CircleId == circleId) && (m.FullPath == fullPath) && 
              (m.Circle.OwnerId == uid)));
            if (circleAuthorizationToFile == null)
            {
                return HttpNotFound();
            }

            return View(circleAuthorizationToFile);
        }

        // GET: MyFSRules/Create
        public IActionResult Create()
        {
            var uid = User.GetUserId();
            var userCircles = _context.Circle.Where(c=>c.OwnerId == uid);
            ViewBag.CircleId = new SelectList(userCircles, "Id", "Name");
            var uccount = userCircles.Count();
            _logger.LogInformation($"User circle count : {uccount}");
            return View();
        }

        // POST: MyFSRules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CircleAuthorizationToFile circleAuthorizationToFile)
        {
            var uid = User.GetUserId();
            if (ModelState.IsValid)
            {
                //  refuse to allow files to other circle than user's ones.
                var circle = await _context.Circle.SingleOrDefaultAsync(c=>c.Id==circleAuthorizationToFile.CircleId);
                if (circle.OwnerId != uid) return this.HttpUnauthorized();
                _context.CircleAuthorizationToFile.Add(circleAuthorizationToFile);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            var userCircles = _context.Circle.Where(c=>c.OwnerId == uid);
            ViewBag.CircleId = new SelectList(userCircles, "Id", "Name");
            return View(circleAuthorizationToFile);
        }

        // GET: MyFSRules/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long circleId, string fullPath)
        {
            var uid = User.GetUserId();
            CircleAuthorizationToFile circleAuthorizationToFile = 
            await _context.CircleAuthorizationToFile
            .Include(a=>a.Circle).SingleOrDefaultAsync(m => m.CircleId == circleId && m.FullPath == fullPath);
            if (circleAuthorizationToFile == null)
            {
                return HttpNotFound();
            }
            if (circleAuthorizationToFile.Circle.OwnerId != uid) return HttpUnauthorized();
            return View(circleAuthorizationToFile);
        }

        // POST: MyFSRules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long circleId, string fullPath)
        {
            var uid = User.GetUserId();
            CircleAuthorizationToFile circleAuthorizationToFile = 
            await _context.CircleAuthorizationToFile
            .Include(a=> a.Circle)
            .SingleOrDefaultAsync(m => m.CircleId == circleId && m.FullPath == fullPath);
            if (circleAuthorizationToFile == null)
            {
                return HttpNotFound();
            }
            if (circleAuthorizationToFile.Circle.OwnerId != uid) return HttpUnauthorized();
            _context.CircleAuthorizationToFile.Remove(circleAuthorizationToFile);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
