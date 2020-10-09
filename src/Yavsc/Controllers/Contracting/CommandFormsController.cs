using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Workflow;

namespace Yavsc.Controllers
{
    public class CommandFormsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommandFormsController(ApplicationDbContext context)
        {
            _context = context;
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
                return HttpNotFound();
            }

            CommandForm commandForm = await _context.CommandForm.SingleAsync(m => m.Id == id);
            if (commandForm == null)
            {
                return HttpNotFound();
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
            ViewBag.ActionName = Startup.Forms.Select(c => new SelectListItem { Value = c, Text = c, Selected = (commandForm?.ActionName == c) });
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
                return HttpNotFound();
            }

            CommandForm commandForm = await _context.CommandForm.SingleAsync(m => m.Id == id);
            if (commandForm == null)
            {
                return HttpNotFound();
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
                return HttpNotFound();
            }

            CommandForm commandForm = await _context.CommandForm.SingleAsync(m => m.Id == id);
            if (commandForm == null)
            {
                return HttpNotFound();
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
