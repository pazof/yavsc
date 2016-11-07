using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using Yavsc.Models;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Yavsc.Models.Booking;
using Yavsc.Helpers;
using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Mvc.ViewEngines;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Extensions.OptionsModel;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Yavsc.Controllers
{
    [ServiceFilter(typeof(LanguageActionFilter)),
    Route("do")]
    public class FrontOfficeController : Controller
    {
        ApplicationDbContext _context;
        UserManager<ApplicationUser> _userManager;

        ILogger _logger;
        public FrontOfficeController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<FrontOfficeController>();
        }
        public ActionResult Index()
        {
            var latestPosts = _context.Blogspot.Where(
                x => x.Visible == true
            ).OrderBy(x => x.Modified).Take(25).ToArray();
            return View(latestPosts);
        }

        [Route("Book/{id?}"), HttpGet]
        public ActionResult Book(string id)
        {
            if (id == null)
            {
                throw new NotImplementedException("No Activity code");
            }

            ViewBag.Activities = _context.ActivityItems(id);
            ViewBag.Activity = _context.Activities.FirstOrDefault(
                a => a.Code == id);

            return View(
                _context.Performers.Include(p => p.Performer).Where
                (p => p.ActivityCode == id && p.Active).OrderBy(
                    x => x.MinDailyCost
                )
            );
        }

        [Route("Book/{id}"), HttpPost]
        public ActionResult Book(BookQuery bookQuery)
        {
            if (ModelState.IsValid)
            {
                var pro = _context.Performers.Include(
                    pr => pr.Performer
                ).FirstOrDefault(
                    x => x.PerformerId == bookQuery.PerformerId
                );
                if (pro == null)
                    return HttpNotFound();
                // Let's create a command
                if (bookQuery.Id == 0)
                {
                    _context.BookQueries.Add(bookQuery);
                }
                else
                {
                    _context.BookQueries.Update(bookQuery);
                }
                _context.SaveChanges();
                // TODO Send sys notifications &
                // notify the user (make him a basket badge)
                return View("Index");
            }
            ViewBag.Activities = _context.ActivityItems(null);
            return View(_context.Performers.Include(p => p.Performer).Where
                (p => p.Active).OrderBy(
                    x => x.MinDailyCost
                ));
        }

        [Produces("text/x-tex"), Authorize, Route("estimate-{id}.tex")]
        public ViewResult EstimateTex(long id)
        {
            var estimate = _context.Estimates.Include(x => x.Query)
            .Include(x => x.Query.Client)
            .Include(x => x.Query.PerformerProfile)
            .Include(x => x.Query.PerformerProfile.OrganizationAddress)
            .Include(x => x.Query.PerformerProfile.Performer)
            .Include(e => e.Bill).FirstOrDefault(x => x.Id == id);
            Response.ContentType = "text/x-tex";
            return View("Estimate.tex", estimate);
        }

        class TeOtions : IOptions<MvcViewOptions>
        {
            public MvcViewOptions Value
            {
                get
                {
                    return new MvcViewOptions();
                }
            }
        }

        [Authorize,Route("Estimate-{id}.pdf")]
        public async Task<IActionResult> EstimatePdf(long id)
        {
            ViewBag.TempDir = Startup.SiteSetup.TempDir;
            ViewBag.BillsDir = Startup.UserBillsDirName;
        
            var estimate = _context.Estimates.Include(x => x.Query)
            .Include(x => x.Query.Client)
            .Include(x => x.Query.PerformerProfile)
            .Include(x => x.Query.PerformerProfile.OrganizationAddress)
            .Include(x => x.Query.PerformerProfile.Performer)
            .Include(e => e.Bill).FirstOrDefault(x => x.Id == id); 
            if (estimate==null)
                throw new Exception("No data");
            
            return View("Estimate.pdf",estimate);
            /* 
            await result.ExecuteResultAsync(ActionContext);
            var cd = new System.Net.Mime.ContentDisposition
            {
                // for example foo.bak
                FileName = $"estimate-{id}.pdf",

                // always prompt the user for downloading, set to true if you want 
                // the browser to try to show the file inline
                Inline = false,
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return File((Byte[])ViewData["Pdf"], "application/x-pdf"); */
        }
 /* 

        [Produces("application/x-pdf"), Authorize, Route("testimate-{id}.pdf")]
        public async Task<IActionResult> BadEstimatePdf(long id)
        {

            var tempDir = Startup.SiteSetup.TempDir;

            string name = $"tmpestimtex-{id}";
            string fullname = new FileInfo(
                System.IO.Path.Combine(tempDir, name)).FullName;
            var writer = new System.IO.StringWriter();
            try
    {
        using (StringWriter sw = new StringWriter())
        {
            Microsoft.AspNet.Mvc.ViewEngines.CompositeViewEngine ve = new CompositeViewEngine(
                new TeOtions {}
            );
            
            ViewEngineResult viewResult = ve.FindPartialView(ActionContext, $"estimate-{id}.tex");
            ViewContext viewContext = new ViewContext(); // ActionContext, viewResult.View, ViewData, TempData, sw);
            await viewResult.View.RenderAsync(viewContext);
        }
    } catch (Exception ex)
    {
        
    }




            FileInfo fo = new FileInfo(fullname + ".pdf");
            if (!fi.Exists)
            {
                throw new Exception("Source write failed");
            }
            using (Process p = new Process())
            {
                p.StartInfo.WorkingDirectory = tempDir;
                p.StartInfo = new ProcessStartInfo();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = "/usr/bin/texi2pdf";
                p.StartInfo.Arguments = $"--batch --build-dir=.";
                
                p.Start();
                            
            using (p.StandardInput)
            {
                
            }
                p.WaitForExit();
                if (p.ExitCode != 0)
                {
                    throw new Exception("Pdf generation failed with exit code:" + p.ExitCode);
                }
            }
            byte[] pdf = null;
            if (fo.Exists)
            {
                using (StreamReader sr = new StreamReader(fo.FullName))
                {
                    pdf = System.IO.File.ReadAllBytes(fo.FullName);
                }
                fo.Delete();
            }
            fi.Delete();


            var cd = new System.Net.Mime.ContentDisposition
            {
                // for example foo.bak
                FileName = $"estimate-{id}.pdf",

                // always prompt the user for downloading, set to true if you want 
                // the browser to try to show the file inline
                Inline = false,
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return File(pdf, "application/x-pdf");
        } */
    } 
}