using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Yavsc.Models;
using Yavsc.Models.Billing;

namespace Yavsc.Controllers
{
    [Authorize(Roles="Administrator")]
    public class SIRENExceptionsController : Controller
    {
        private ApplicationDbContext _context;

        public SIRENExceptionsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: SIRENExceptions
        public IActionResult Index()
        {
            return View(_context.ExceptionsSIREN.ToList());
        }

        // GET: SIRENExceptions/Details/5
        public IActionResult Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            ExceptionSIREN exceptionSIREN = _context.ExceptionsSIREN.Single(m => m.SIREN == id);
            if (exceptionSIREN == null)
            {
                return HttpNotFound();
            }

            return View(exceptionSIREN);
        }

        // GET: SIRENExceptions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SIRENExceptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ExceptionSIREN exceptionSIREN)
        {
            if (ModelState.IsValid)
            {
                _context.ExceptionsSIREN.Add(exceptionSIREN);
                _context.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(exceptionSIREN);
        }

        // GET: SIRENExceptions/Edit/5
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            ExceptionSIREN exceptionSIREN = _context.ExceptionsSIREN.Single(m => m.SIREN == id);
            if (exceptionSIREN == null)
            {
                return HttpNotFound();
            }
            return View(exceptionSIREN);
        }

        // POST: SIRENExceptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ExceptionSIREN exceptionSIREN)
        {
            if (ModelState.IsValid)
            {
                _context.Update(exceptionSIREN);
                _context.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(exceptionSIREN);
        }

        // GET: SIRENExceptions/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            ExceptionSIREN exceptionSIREN = _context.ExceptionsSIREN.Single(m => m.SIREN == id);
            if (exceptionSIREN == null)
            {
                return HttpNotFound();
            }

            return View(exceptionSIREN);
        }

        // POST: SIRENExceptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            ExceptionSIREN exceptionSIREN = _context.ExceptionsSIREN.Single(m => m.SIREN == id);
            _context.ExceptionsSIREN.Remove(exceptionSIREN);
            _context.SaveChanges(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
