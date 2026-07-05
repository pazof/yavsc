using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Yavsc.Models;
using Yavsc.Models.Workflow;
using Yavsc.Server.Helpers;
using Yavsc.Services;

namespace Yavsc.Controllers
{
    public class CommandFormsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<CommandFormsController> _localizer;

        public CommandFormsController(ApplicationDbContext context,
        IStringLocalizer<CommandFormsController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        // GET: CommandForms
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.CommandForm.Include(c => c.Context);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: CommandForms/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CommandForm commandForm = await _context.CommandForm.SingleAsync(m => m.Id == id);
            if (commandForm == null)
            {
                return NotFound();
            }

            return View(commandForm);
        }

        // GET: CommandForms/Create
        public IActionResult Create()
        {
            SetViewBag();
            return View();
        }

        private void SetViewBag(CommandForm commandForm = null)
        {
            ViewBag.ActivityCode = new SelectList(_context.Activities, "Code", "Name", commandForm?.ActivityCode);
            ViewBag.ActionName = BillingService.Billing.Keys
                .Select((string b) => new SelectListItem { Value = b, Text = _localizer[b] }).ToList();
        }

        // POST: CommandForms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommandForm commandForm)
        {
            if (ModelState.IsValid)
            {
                _context.CommandForm.Add(commandForm);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            SetViewBag(commandForm);
            return View(commandForm);
        }

        // GET: CommandForms/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CommandForm commandForm = await _context.CommandForm.SingleAsync(m => m.Id == id);
            if (commandForm == null)
            {
                return NotFound();
            }
            SetViewBag(commandForm);
            return View(commandForm);
        }

        // POST: CommandForms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CommandForm commandForm)
        {
            if (ModelState.IsValid)
            {
                _context.Update(commandForm);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            SetViewBag(commandForm);
            return View(commandForm);
        }

        // GET: CommandForms/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CommandForm commandForm = await _context.CommandForm.SingleAsync(m => m.Id == id);
            if (commandForm == null)
            {
                return NotFound();
            }

            return View(commandForm);
        }

        // POST: CommandForms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            CommandForm commandForm = await _context.CommandForm.SingleAsync(m => m.Id == id);
            _context.CommandForm.Remove(commandForm);
            await _context.SaveChangesAsync(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
