using System.Threading.Tasks;
using Yavsc.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yavsc.Models;
using Yavsc.Models.Messaging;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Yavsc.Controllers
{
    public class AnnouncesController : Controller
    {
        private readonly ApplicationDbContext _context;
        readonly IStringLocalizer<AnnouncesController> _localizer;
        readonly IAuthorizationService _authorizationService;

        public AnnouncesController(ApplicationDbContext context, 
        IAuthorizationService authorizationService,
        IStringLocalizer<AnnouncesController> localizer)
        {
            _context = context;    
            _authorizationService = authorizationService;
            _localizer = localizer;
        }

        // GET: Announces
        public async Task<IActionResult> Index()
        {
            return View(await _context.Announce.ToListAsync());
        }

        // GET: Announces/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Announce announce = await _context.Announce.SingleAsync(m => m.Id == id);
            if (announce == null)
            {
                return NotFound();
            }

            return View(announce);
        }

        // GET: Announces/Create
        public async Task<IActionResult> Create()
        {
            var model = new Announce();
            await SetupView(model);
            return View(model);
        }
        private async Task SetupView(Announce announce)
        {
            ViewBag.IsAdmin = User.IsInRole(Constants.AdminGroupName);
            ViewBag.IsPerformer = User.IsInRole(Constants.PerformerGroupName);
            ViewBag.AllowEdit = announce==null || announce.Id<=0 || !_authorizationService.AuthorizeAsync(User,announce,new EditPermission()).IsFaulted;
            List<SelectListItem> dl = new List<SelectListItem>();
            var rnames = System.Enum.GetNames(typeof(Reason));
            var rvalues = System.Enum.GetValues(typeof(Reason));
            
            for (int i = 0; i<rnames.Length; i++) {
                dl.Add(new SelectListItem { Text = 
                _localizer[rnames[i]], 
                Value= rvalues.GetValue(i).ToString() });
            }

            ViewBag.For = dl.ToArray();
        }
        // POST: Announces/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announce announce)
        {
            await SetupView(announce);
            if (ModelState.IsValid)
            {
                // Only allow admin to create corporate annonces
                if (announce.For == Reason.Corporate && ! ViewBag.IsAdmin)
                {
                    ModelState.AddModelError("For", _localizer["YourNotAdmin"]);
                    return View(announce);
                }

                // Only allow performers to create ServiceProposal 
                if (announce.For == Reason.ServiceProposal && ! ViewBag.IsAdmin)
                {
                    ModelState.AddModelError("For", _localizer["YourNotAPerformer"]);
                    return View(announce);
                }

                _context.Announce.Add(announce);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(announce);
        }

        // GET: Announces/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Announce announce = await _context.Announce.SingleAsync(m => m.Id == id);
            if (announce == null)
            {
                return NotFound();
            }
            return View(announce);
        }

        // POST: Announces/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Announce announce)
        {
            if (ModelState.IsValid)
            {
                _context.Update(announce);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(announce);
        }

        // GET: Announces/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Announce announce = await _context.Announce.SingleAsync(m => m.Id == id);
            if (announce == null)
            {
                return NotFound();
            }

            return View(announce);
        }

        // POST: Announces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            Announce announce = await _context.Announce.SingleAsync(m => m.Id == id);
            _context.Announce.Remove(announce);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
