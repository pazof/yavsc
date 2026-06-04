using IdentityServer8.EntityFramework.DbContexts;
using IdentityServer8.EntityFramework.Entities;
using IdentityServer8.EntityFramework.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yavsc.Models;
using Yavsc.Models.Auth;
using Yavsc.Server.Helpers;

namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ClientStore clientStore;
    private readonly SiteSettings siteSettings;

    public ClientController(
            ApplicationDbContext dbContext,
    ClientStore clientStore, IOptions<SiteSettings> siteSettingsOptions


            )
        {
            this.dbContext = dbContext;
            this.clientStore = clientStore;
            this.siteSettings = siteSettingsOptions.Value;
        }

        // GET: Client
        public async Task<IActionResult> Index()
        {
            return View(await dbContext.Clients.Include(c => c.AllowedGrantTypes)
            .Include(c => c.RedirectUris).ToListAsync());
        }

        // GET: Client/Details/5
        public async Task<IActionResult> Details(int id)
        {

            Client client = await dbContext.Clients.Include(
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

                dbContext.Clients.Add(client);
                await dbContext.SaveChangesAsync(User.GetUserId());

                dbContext.ClientRedirectUris.Add(new ClientRedirectUri
                {
                    ClientId = client.Id,
                    RedirectUri = siteSettings.Audience

                });
                dbContext.ClientCorsOrigins.Add(new ClientCorsOrigin
                {
                    ClientId = client.Id,
                    Origin = siteSettings.Audience

                });
                foreach (String credType in new String[] { "code", "client_credentials", "password" })
                {
                    dbContext.ClientGrantTypes.Add(new ClientGrantType
                    {
                        ClientId = client.Id,
                        GrantType = credType
                    });
                }
                foreach (String scope in new String[] { "openid", "profile" })
                {
                    dbContext.ClientScopes.Add(new ClientScope
                    {
                        ClientId = client.Id,
                        Scope = scope
                    });
                }
                
                await dbContext.SaveChangesAsync(User.GetUserId());

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
            ViewBag.AccessTokenType = types;
        }
        // GET: Client/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            Client client = await dbContext.Clients.SingleOrDefaultAsync(m => m.Id == id);
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
                        dbContext.Update(secret);
                    }
                }
                dbContext.Update(client);
                await dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(client);
        }

        // GET: Client/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(int id)
        {

            Client client = await dbContext.Clients.SingleOrDefaultAsync(m => m.Id == id);
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
            Client client = await dbContext.Clients
            .Include(client => client.ClientSecrets)
            .SingleAsync(m => m.Id == id);
            dbContext.Clients.Remove(client);
            await dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
