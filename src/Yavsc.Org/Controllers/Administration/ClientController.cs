using IdentityServer8.EntityFramework.DbContexts;
using IdentityServer8.EntityFramework.Entities;
using IdentityServer8.EntityFramework.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Auth;

namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ClientStore clientStore;

        public ClientController(ApplicationDbContext context,
    ClientStore clientStore,

    IdentityServer8.Stores.ValidatingClientStore<ClientStore> validatingClientStore


            )
        {
            this.context = context;
            this.clientStore = clientStore;
        }

        // GET: Client
        public async Task<IActionResult> Index()
        {
            return View(await context.Clients.Include(c => c.AllowedGrantTypes)
            .Include(c => c.RedirectUris).ToListAsync());
        }

        // GET: Client/Details/5
        public async Task<IActionResult> Details(int id)
        {

            Client client = await context.Clients.Include(
                c => c.ClientSecrets
            ).Include(c => c.AllowedGrantTypes)
            .Include(c => c.RedirectUris)
            .Include(c=>c.ClientSecrets)
            .Include(c=>c.AllowedCorsOrigins)
            .Include(c=>c.AllowedScopes)
            .Include(c=>c.IdentityProviderRestrictions)
            .Include(c=>c.PostLogoutRedirectUris)
            .SingleAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // GET: Client/Create
        public IActionResult Create()
        {
            Secret s;

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
                var model = await clientStore.FindClientByIdAsync(client.ClientId);
                if (model != null)
                {
                    ModelState.AddModelError("ClientId", "existent");
                    return BadRequest(ModelState);
                }

                context.Clients.Add(client);
                if (client.ClientSecrets != null)
                {
                    foreach (var secret in client.ClientSecrets)
                    {
                        context.ClientSecrets.Add(secret);
                    }
                }
                await context.SaveChangesAsync();


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
        public async Task<IActionResult> Edit(int id)
        {
            Client client = await context.Clients.SingleOrDefaultAsync(m => m.Id == id);
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

                if (client.ClientSecrets != null)
                {
                    foreach (var secret in client.ClientSecrets)
                    {
                        context.Update(secret);
                    }
                }
                context.Update(client);
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(client);
        }

        // GET: Client/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(int id)
        {

            Client client = await context.Clients.SingleOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Client client = await context.Clients
            .Include(client => client.ClientSecrets)
            .SingleAsync(m => m.Id == id);
            context.Clients.Remove(client);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
