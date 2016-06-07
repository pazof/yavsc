using System;
using System.Linq;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class ApplicationController : Controller
    {
        private ApplicationDbContext _context;

        public ApplicationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Application
        public IActionResult Index()
        {
            return View(_context.Applications.ToList());
        }

        // GET: Application/Details/5
        public IActionResult Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Application application = _context.Applications.Single(m => m.ApplicationID == id);
            if (application == null)
            {
                return HttpNotFound();
            }

            return View(application);
        }

        // GET: Application/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Application/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Application application)
        {
            if (ModelState.IsValid)
            {
                application.ApplicationID = Guid.NewGuid().ToString();
                _context.Applications.Add(application);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(application);
        }

        // GET: Application/Edit/5
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Application application = _context.Applications.Single(m => m.ApplicationID == id);
            if (application == null)
            {
                return HttpNotFound();
            }
            return View(application);
        }

        // POST: Application/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Application application)
        {
            if (ModelState.IsValid)
            {
                _context.Update(application);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(application);
        }

        // GET: Application/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Application application = _context.Applications.Single(m => m.ApplicationID == id);
            if (application == null)
            {
                return HttpNotFound();
            }

            return View(application);
        }

        // POST: Application/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            Application application = _context.Applications.Single(m => m.ApplicationID == id);
            _context.Applications.Remove(application);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
