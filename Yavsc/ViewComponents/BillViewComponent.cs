using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Yavsc.Abstract.FileSystem;
using Yavsc.Billing;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.ViewModels;
using Yavsc.ViewModels.Gen;
using Yavsc.Services;

namespace Yavsc.ViewComponents
{
    public class BillViewComponent : ViewComponent
    {
        ApplicationDbContext dbContext;
        IBillingService billing;
        IStringLocalizer<Yavsc.Resources.YavscLocalisation> localizer;
        ILogger logger ;

        public BillViewComponent(ApplicationDbContext dbContext, 
            IStringLocalizer<Yavsc.Resources.YavscLocalisation> localizer,
            IBillingService billing,
            ILoggerFactory loggerFactory)
        {
            this.billing = billing;
            this.dbContext = dbContext;
            this.localizer = localizer;
            logger = loggerFactory.CreateLogger<BillViewComponent>();
        }

        public async Task<IViewComponentResult> InvokeAsync(string code, IBillable billable, OutputFormat format, bool asBill)
        {
            var di = new DirectoryInfo(Startup.SiteSetup.Bills); 
            var dia = new DirectoryInfo(Startup.SiteSetup.Avatars); 
            ViewBag.BillsDir = di.FullName;
            ViewBag.AvatarsDir = dia.FullName;
            ViewBag.AsBill = asBill; // vrai pour une facture, sinon, c'est un devis
            ViewBag.Acquitted = billable.GetIsAcquitted();

            ViewBag.BillingCode = code;
            var client = await dbContext.Users
            .Include(u=>u.PostalAddress)
            .SingleAsync(u=>u.Id == billable.ClientId);
            ViewBag.Client = client;
            var performer = await dbContext.Users
            .Include(u=>u.BankInfo)
            .Include(u=>u.PostalAddress)
            .SingleAsync(u=>u.Id == billable.PerformerId);
            ViewBag.Performer = performer;
            string clientAddress = client.PostalAddress?.Address ?? null;
            ViewBag.ClientAddress = clientAddress.SplitAddressToTeX();


            var profile = await dbContext.Performers
            .Include(p=>p.OrganizationAddress)
            .SingleAsync(p=>p.PerformerId == billable.PerformerId);
            ViewBag.PerformerProfile = profile;
            ViewBag.ActivityLabel = (await dbContext.Activities.SingleAsync(a => a.Code == billable.ActivityCode)).Name;

            var proaddr = profile.OrganizationAddress.Address;
            ViewBag.PerformerOrganizationAddress = proaddr.SplitAddressToTeX() ;
            ViewBag.FooterPerformerOrganizationAddress = proaddr.SplitAddressToTeX(", ");

            ViewBag.PerformerAddress = performer.PostalAddress?.Address.SplitAddressToTeX() ;
            switch (format) {
                case OutputFormat.LaTeX :
                    return this.View("Bill_tex", billable);
                case OutputFormat.Pdf :
                    string tex = null;
                    var oldWriter = ViewComponentContext.ViewContext.Writer;
                    using (var writer = new StringWriter())
                    {
                        this.ViewComponentContext.ViewContext.Writer = writer;
                        var resultTex = View("Bill_tex", billable);
                        await resultTex.ExecuteAsync(this.ViewComponentContext);
                        tex = writer.ToString();
                    }
                    ViewComponentContext.ViewContext.Writer = oldWriter;
                    
                    var genrtrData = new PdfGenerationViewModel{ 
                            Temp = Startup.Temp,
                            TeXSource = tex, 
                            DestDir = AbstractFileSystemHelpers.UserBillsDirName,
                            BaseFileName = billable.GetFileBaseName(billing)
                        };
                    if (genrtrData.GenerateEstimatePdf()) {
                        return Json(new { Generated = genrtrData.BaseFileName+".pdf" });
                    } else {
                        return Json(new { Error = genrtrData.GenerationErrorMessage } );
                    }
            }
            ViewBag.BillFileInfo =  billable.GetBillInfo(billing);
            return View("Default",billable);
           
        }

    }
}
