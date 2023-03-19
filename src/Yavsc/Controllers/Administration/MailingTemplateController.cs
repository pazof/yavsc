
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Yavsc.Models;
using Yavsc.Models.Calendar;
using Yavsc.Server.Models.EMailing;
using Microsoft.AspNetCore.Authorization;
using Yavsc.Server.Settings;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;

namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class MailingTemplateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger logger;

        public MailingTemplateController(ApplicationDbContext context,
         ILoggerFactory loggerFactory)
        {
            _context = context;
            logger = loggerFactory.CreateLogger<MailingTemplateController>();

        }

        // GET: MailingTemplate
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.MailingTemplate;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: MailingTemplate/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MailingTemplate mailingTemplate = await _context.MailingTemplate.SingleAsync(m => m.Id == id);
            if (mailingTemplate == null)
            {
                return NotFound();
            }

            return View(mailingTemplate);
        }

        List<SelectListItem> GetSelectFromEnum(Type enumType)
        {

            var list = new List<SelectListItem>();
            foreach (var v in enumType.GetEnumValues())
             {
                 list.Add(new SelectListItem { Value = v.ToString(), Text = enumType.GetEnumName(v) });
             }
          return list;
  
        }

        private void SetupViewBag()
        {
            ViewBag.ManagerId = new SelectList(_context.ApplicationUser, "Id", "UserName");
            ViewBag.ToSend = GetSelectFromEnum(typeof(Periodicity));
            ViewBag.Id = UserPolicies.Criterias.Select(
                    c => new SelectListItem{ Text = c.Key, Value = c.Key }).ToList();
        }

        // GET: MailingTemplate/Create
        public IActionResult Create()
        {
            SetupViewBag();
            return View();
        }

        // POST: MailingTemplate/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MailingTemplate mailingTemplate)
        {
            if (ModelState.IsValid)
            {
                _context.MailingTemplate.Add(mailingTemplate);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            SetupViewBag();
            return View(mailingTemplate);
        }

        // GET: MailingTemplate/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MailingTemplate mailingTemplate = await _context.MailingTemplate.SingleAsync(m => m.Id == id);
            if (mailingTemplate == null)
            {
                return NotFound();
            }
            SetupViewBag();
            return View(mailingTemplate);
        }

        // POST: MailingTemplate/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MailingTemplate mailingTemplate)
        {

            if (ModelState.IsValid)
            {
                _context.Update(mailingTemplate);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            SetupViewBag();
            return View(mailingTemplate);
        }

        // GET: MailingTemplate/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MailingTemplate mailingTemplate = await _context.MailingTemplate.SingleAsync(m => m.Id == id);
            if (mailingTemplate == null)
            {
                return NotFound();
            }

            return View(mailingTemplate);
        }

        // POST: MailingTemplate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            MailingTemplate mailingTemplate = await _context.MailingTemplate.SingleAsync(m => m.Id == id);
            _context.MailingTemplate.Remove(mailingTemplate);
            await _context.SaveChangesAsync(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
