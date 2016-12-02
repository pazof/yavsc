using System.IO;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using System.Web.Routing;

namespace Yavsc.ApiControllers
{
    using Models;
    using Helpers;
    using System.Linq;
    using Microsoft.Data.Entity;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    [Route("api/pdfestimate"), Authorize]
    public class PdfEstimateController : Controller
    {
        ApplicationDbContext dbContext;
        private IAuthorizationService authorizationService;

        private ILogger logger;

        public PdfEstimateController(
            IAuthorizationService authorizationService,
            ILoggerFactory loggerFactory,
         ApplicationDbContext context)
        {
            this.authorizationService = authorizationService;
            dbContext = context;
            logger = loggerFactory.CreateLogger<PdfEstimateController>();
        }


        [HttpGet("get/{id}", Name = "Get"), Authorize]
        public async Task<IActionResult> Get(long id)
        {
            var estimate = dbContext.Estimates.Include(
                e=>e.Query
            ).FirstOrDefault(e=>e.Id == id);
            logger.LogWarning($"#######ESTIMATE OWNER ID {estimate.OwnerId} ########");
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }

            var filename = $"estimate-{id}.pdf";

            var cd = new System.Net.Mime.ContentDisposition
            {
                // for example foo.bak
                FileName = filename,

                // always prompt the user for downloading, set to true if you want 
                // the browser to try to show the file inline
                Inline = false,
            };

            FileInfo fi = new FileInfo(Path.Combine(Startup.UserBillsDirName, filename));
            if (!fi.Exists) return Ok(new { Error = "Not generated" });
            return File(fi.OpenRead(), "application/x-pdf", filename); ;
        }

        [HttpGet("estimate-{id}.tex", Name = "GetTex"), Authorize]
        public IActionResult GetTex(long id)
        {
            Response.ContentType = "text/x-tex";
            return ViewComponent("Estimate",new object[] { id, "LaTeX" });
        }

        [HttpPost("gen/{id}")]
        public IActionResult GeneratePdf(long id)
        {
            return ViewComponent("Estimate",new object[] { id, "Pdf" } );
        }

        [HttpPost("prosign/{id}")]
        public IActionResult ProSign(long id)
        {
            if (Request.Form.Files.Count!=1)
                return new BadRequestResult();
            return Ok (User.ReceiveProSignature(id,Request.Form.Files[0]));
        }
    }
}