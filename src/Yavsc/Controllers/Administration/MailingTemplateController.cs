using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Calendar;
using Yavsc.Server.Models.EMailing;
using Microsoft.AspNet.Authorization;


namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class MailingTemplateController : Controller
    {
        private ApplicationDbContext _context;

        public MailingTemplateController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: MailingTemplate
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.MailingTemplate.Include(m => m.Manager);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: MailingTemplate/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            MailingTemplate mailingTemplate = await _context.MailingTemplate.SingleAsync(m => m.Id == id);
            if (mailingTemplate == null)
            {
                return HttpNotFound();
            }

            return View(mailingTemplate);
        }


        List<SelectListItem> GetSelectFromEnum(Type enumType )
        {

            var list = new List<SelectListItem>();
            foreach (var v in enumType.GetEnumValues())
             {
                 list.Add(new SelectListItem { Value = v.ToString(), Text = enumType.GetEnumName(v) });
             }
          return list;
  
        }


        // GET: MailingTemplate/Create
        public IActionResult Create()
        {
            ViewBag.ManagerId = new SelectList(_context.ApplicationUser, "Id", "UserName");
            ViewBag.ToSend = GetSelectFromEnum(typeof(Periodicity));
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
            ViewBag.ManagerId = new SelectList(_context.ApplicationUser, "Id", "UserName");
            ViewBag.ToSend = GetSelectFromEnum(typeof(Periodicity));
            return View(mailingTemplate);
        }

        // GET: MailingTemplate/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            MailingTemplate mailingTemplate = await _context.MailingTemplate.SingleAsync(m => m.Id == id);
            if (mailingTemplate == null)
            {
                return HttpNotFound();
            }
            ViewBag.ManagerId = new SelectList(_context.ApplicationUser, "Id", "UserName");
            ViewBag.ToSend = GetSelectFromEnum(typeof(Periodicity));
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
            ViewBag.ManagerId = new SelectList(_context.ApplicationUser, "Id", "UserName");
            ViewBag.ToSend = GetSelectFromEnum(typeof(Periodicity));
            return View(mailingTemplate);
        }

        // GET: MailingTemplate/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            MailingTemplate mailingTemplate = await _context.MailingTemplate.SingleAsync(m => m.Id == id);
            if (mailingTemplate == null)
            {
                return HttpNotFound();
            }

            return View(mailingTemplate);
        }

        // POST: MailingTemplate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            MailingTemplate mailingTemplate = await _context.MailingTemplate.SingleAsync(m => m.Id == id);
            _context.MailingTemplate.Remove(mailingTemplate);
            await _context.SaveChangesAsync(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
