using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Server.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class SIRENExceptionsController : Controller
    {
        private readonly ApplicationDbContext _context;

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
                return NotFound();
            }

            ExceptionSIREN exceptionSIREN = _context.ExceptionsSIREN.Single(m => m.SIREN == id);
            if (exceptionSIREN == null)
            {
                return NotFound();
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
            exceptionSIREN ??= new ExceptionSIREN();
            exceptionSIREN.SIREN = NormalizeSiren(exceptionSIREN.SIREN);

            if (string.IsNullOrWhiteSpace(exceptionSIREN.SIREN) || exceptionSIREN.SIREN.Length != 9 || !exceptionSIREN.SIREN.All(char.IsDigit))
            {
                ModelState.AddModelError(nameof(ExceptionSIREN.SIREN), "Le SIREN doit contenir exactement 9 chiffres.");
            }

            if (_context.ExceptionsSIREN.Any(e => e.SIREN == exceptionSIREN.SIREN))
            {
                ModelState.AddModelError(nameof(ExceptionSIREN.SIREN), "Ce SIREN est deja dans la liste des exceptions.");
            }

            if (ModelState.IsValid)
            {
                _context.ExceptionsSIREN.Add(exceptionSIREN);
                try
                {
                    _context.SaveChanges(User.GetUserId());
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(string.Empty, "Impossible d'enregistrer cette exception SIREN.");
                }
            }
            return View(exceptionSIREN);
        }

        private static string NormalizeSiren(string? siren)
        {
            if (string.IsNullOrWhiteSpace(siren)) return string.Empty;
            return new string(siren.Where(char.IsDigit).ToArray());
        }

        // GET: SIRENExceptions/Edit/5
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ExceptionSIREN exceptionSIREN = _context.ExceptionsSIREN.Single(m => m.SIREN == id);
            if (exceptionSIREN == null)
            {
                return NotFound();
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
                return NotFound();
            }

            ExceptionSIREN exceptionSIREN = _context.ExceptionsSIREN.Single(m => m.SIREN == id);
            if (exceptionSIREN == null)
            {
                return NotFound();
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
