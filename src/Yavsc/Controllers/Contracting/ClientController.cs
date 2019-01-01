using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using System.Collections.Generic;
using Yavsc.Models;
using Yavsc.Models.Auth;
using System.Security.Claims;

namespace Yavsc.Controllers
{
    public class ClientController : Controller
    {
        private ApplicationDbContext _context;

        public ClientController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Client
        public async Task<IActionResult> Index()
        {
            return View(await _context.Applications.ToListAsync());
        }

        // GET: Client/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Client client = await _context.Applications.SingleAsync(m => m.Id == id);
            if (client == null)
            {
                return HttpNotFound();
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
                client.Id = Guid.NewGuid().ToString();
                _context.Applications.Add(client);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            SetAppTypesInputValues();
            return View(client);
        }
        private void SetAppTypesInputValues()
        {
            IEnumerable<SelectListItem> types =  new SelectListItem[] {
                 new SelectListItem {
                     Text = ApplicationTypes.JavaScript.ToString(),
                     Value = ((int) ApplicationTypes.JavaScript).ToString() },
                 new SelectListItem {
                     Text = ApplicationTypes.NativeConfidential.ToString(),
                     Value = ((int) ApplicationTypes.NativeConfidential).ToString() 
                     }
             };
            ViewData["Type"] = types;
        }
        // GET: Client/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Client client = await _context.Applications.SingleAsync(m => m.Id == id);
            if (client == null)
            {
                return HttpNotFound();
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
                await _context.SaveChangesAsync(User.GetUserId());
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
                return HttpNotFound();
            }

            Client client = await _context.Applications.SingleAsync(m => m.Id == id);
            if (client == null)
            {
                return HttpNotFound();
            }

            return View(client);
        }

        // POST: Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            Client client = await _context.Applications.SingleAsync(m => m.Id == id);
            _context.Applications.Remove(client);
            await _context.SaveChangesAsync(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
