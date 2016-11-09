using System.IO;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using System.Threading.Tasks;
using System.Web.Routing;
using Microsoft.AspNet.Mvc.ViewComponents;
using Microsoft.AspNet.Razor;

namespace Yavsc.ApiControllers
{
    using Models;

    [Route("api/pdfestimate"), Authorize]
    public class PdfEstimateController : Controller
    {
        ApplicationDbContext dbContext;
        DefaultViewComponentHelper helper;
        IViewComponentDescriptorCollectionProvider provider;
        IViewComponentInvokerFactory factory;
        RazorEngineHost host;
        RazorTemplateEngine engine;
        IViewComponentSelector selector;


        public PdfEstimateController(
            IViewComponentDescriptorCollectionProvider provider,
            IViewComponentSelector selector,
        IViewComponentInvokerFactory factory,
         ApplicationDbContext context)
        {
            
            this.selector = selector;
            this.provider = provider;
            this.factory = factory;
            helper = new DefaultViewComponentHelper(provider, selector, factory);
            dbContext = context;

            var language = new CSharpRazorCodeLanguage();
            host = new RazorEngineHost(language)
            {
                DefaultBaseClass = "RazorPage",
                DefaultClassName = "Estimate",
                DefaultNamespace = "Yavsc",
            };

            // Everyone needs the System namespace, right?
            host.NamespaceImports.Add("System");
            engine = new RazorTemplateEngine(host);

            
            /*
            GeneratorResults razorResult =
   engine.GenerateCode(

   ) */
        }


        [HttpGet("get/{id}", Name = "Get"), Authorize]
        public IActionResult Get(long id)
        {
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

        [HttpGet("GetComponents", Name = "GetComponents")]
        public IActionResult GetComponents()
        {
            return Ok(provider);
        }

        [HttpGet("estimate-{id}.tex", Name = "GetTex"), Authorize]
        public IActionResult GetTex(long id)
        {
            Response.ContentType = "text/x-tex";

            return ViewComponent("Estimate",new object[] { id, false });
        }

        [HttpGet("gen/{id}")]
        public async Task<IActionResult> GeneratePdf(long id)
        {
             /*

             using (TextWriter wr = new StringWriter()) {
             ViewComponentContext ctx = new ViewComponentContext(
                 selector.SelectComponent("Estimate"), new object[]{id}, 
                 new ViewContext(),
                 wr
             );

             }

             */

            return ViewComponent("Estimate",new object[] { id, true } );
        }
    }
}