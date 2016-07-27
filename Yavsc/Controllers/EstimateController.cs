using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.OptionsModel;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Billing;

namespace Yavsc.Controllers
{
    public class EstimateController : Controller
    {
        private ApplicationDbContext _context;
        private SiteSettings _site;

        public EstimateController(ApplicationDbContext context, IOptions<SiteSettings> siteSettings)
        {
            _context = context;
            _site = siteSettings.Value;
        }

        // GET: Estimate
        public IActionResult Index()
        {
            return View(_context.Estimates.ToList());
        }

        // GET: Estimate/Details/5
        public IActionResult Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Estimate estimate = _context.Estimates
            .Include(e => e.Query)
            .Include(e => e.Query.PerformerProfile)
            .Include(e => e.Query.PerformerProfile.Performer)
            .Single(m => m.Id == id);
            if (estimate == null)
            {
                return HttpNotFound();
            }

            ViewBag.Files = estimate.GetFileContent(_site.UserFiles.DirName);
            return View(estimate);
        }


        // GET: Estimate/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Estimate/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Estimate estimate,
         ICollection<IFormFile> newGraphics,
         ICollection<IFormFile> newFiles
         )
        {
            if (ModelState.IsValid)
            {
                _context.Estimates
                .Add(estimate);
                _context.SaveChanges();

                var perfomerProfile = _context.Performers
                .Include(
                    perpr => perpr.Performer).FirstOrDefault(
                    x=>x.PerformerId == estimate.Query.PerformerId
                );
                var command = _context.BookQueries.FirstOrDefault(
                    cmd => cmd.Id == estimate.CommandId
                );

                var userdir = Path.Combine(
                    _site.UserFiles.DirName,
                    perfomerProfile.Performer.UserName
               );

                var fsp = new PhysicalFileProvider(userdir);
                var billsdir = Path.Combine(userdir,
                Constants.UserBillsFilesDir);

                foreach (var gr in newGraphics)
                {
                    gr.SaveAs(
                        Path.Combine(
                        Path.Combine(billsdir, estimate.Id.ToString()),
                        gr.ContentDisposition));
                }
                return RedirectToAction("Index");
            }
            return View(estimate);
        }


        private void Save(  ICollection<IFormFile> newGraphics,
         ICollection<IFormFile> newFiles) {

         }
        // GET: Estimate/Edit/5
        public IActionResult Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Estimate estimate = _context.Estimates.Single(m => m.Id == id);
            if (estimate == null)
            {
                return HttpNotFound();
            }
            ViewBag.Files = estimate.GetFileContent(_site.UserFiles.DirName);
            return View(estimate);
        }

        // POST: Estimate/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Estimate estimate)
        {
            if (ModelState.IsValid)
            {
                _context.Update(estimate);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(estimate);
        }

        // GET: Estimate/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Estimate estimate = _context.Estimates.Single(m => m.Id == id);
            if (estimate == null)
            {
                return HttpNotFound();
            }

            return View(estimate);
        }

        // POST: Estimate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            Estimate estimate = _context.Estimates.Single(m => m.Id == id);
            _context.Estimates.Remove(estimate);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
