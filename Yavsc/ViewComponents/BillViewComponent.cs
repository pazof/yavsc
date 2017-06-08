using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Yavsc.Billing;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.ViewModels;
using Yavsc.ViewModels.Gen;

namespace Yavsc.ViewComponents
{
    public class BillViewComponent : ViewComponent
    {
        ApplicationDbContext dbContext;
        IStringLocalizer<Yavsc.Resources.YavscLocalisation> localizer;
        ILogger logger ;

        public BillViewComponent(ApplicationDbContext dbContext, 
            IStringLocalizer<Yavsc.Resources.YavscLocalisation> localizer,
            ILoggerFactory loggerFactory)
        {
            this.dbContext = dbContext;
            this.localizer = localizer;
            logger = loggerFactory.CreateLogger<BillViewComponent>();
        }

        public async Task<IViewComponentResult> InvokeAsync(string code, IBillable billable, OutputFormat format, bool asBill, bool acquitted)
        {
            var di = new DirectoryInfo(Startup.SiteSetup.UserFiles.Bills); 
            var dia = new DirectoryInfo(Startup.SiteSetup.UserFiles.Avatars); 
            ViewBag.BillsDir = di.FullName;
            ViewBag.AvatarsDir = dia.FullName;
            ViewBag.AsBill = asBill; // vrai pour une facture, sinon, c'est un devis
            ViewBag.Acquitted = acquitted;

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
                    
                    return this.View("Bill_pdf", 
                        new PdfGenerationViewModel{ 
                            Temp = Startup.Temp,
                            TeXSource = tex, 
                            DestDir = Startup.UserBillsDirName,
                            BaseFileName = $"facture-{code}-{billable.Id}"
                        } );
            }
            return View("Default",billable);
           
        }

    }
}
