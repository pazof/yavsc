using IdentityServer8.EntityFramework.DbContexts;
using IdentityServer8.EntityFramework.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Auth;
using Yavsc.Server.Helpers;

namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class ClientController : Controller
    {
        private readonly ConfigurationDbContext _context;

        public ClientController(ConfigurationDbContext context)
        {
            _context = context;
        }

        // GET: Client
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clients.Include(c=>c.AllowedGrantTypes)
            .Include(c=>c.RedirectUris).ToListAsync());
        }

        // GET: Client/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Client client = await _context.Clients.Include(
                c => c.ClientSecrets
            ).Include(c=>c.AllowedGrantTypes)
            .Include(c=>c.RedirectUris)
            .SingleAsync(m => m.ClientId == id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // GET: Client/Create
        public IActionResult Create()
        {
            SetAppTypesInputValues();
            return View();
        }

        // POST: Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(client.ClientId))
                    client.ClientId = Guid.NewGuid().ToString();
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            SetAppTypesInputValues();
            return View(client);
        }
        private void SetAppTypesInputValues()
        {
            IEnumerable<SelectListItem> types = new SelectListItem[] {
                 new SelectListItem {
                     Text = ApplicationTypes.JavaScript.ToString(),
                     Value = ((int) ApplicationTypes.JavaScript).ToString() },
                 new SelectListItem {
                     Text = ApplicationTypes.NativeConfidential.ToString(),
                     Value = ((int) ApplicationTypes.NativeConfidential).ToString()
                     }
             };
            ViewData["AccessTokenType"] = types;
        }
        // GET: Client/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Client client = await _context.Clients.SingleAsync(m => m.ClientId == id);
            if (client == null)
            {
                return NotFound();
            }
            SetAppTypesInputValues();
            return View(client);
        }

        // POST: Client/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Update(client);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(client);
        }

        // GET: Client/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Client client = await _context.Clients.SingleAsync(m => m.ClientId == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            Client client = await _context.Clients.SingleAsync(m => m.ClientId == id);
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
