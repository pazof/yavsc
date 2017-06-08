using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Localization;
using Yavsc.Billing;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Services;
using Yavsc.ViewModels;
using Yavsc.ViewModels.Gen;

namespace Yavsc.ViewComponents
{

    public class BillViewComponent : ViewComponent
    {
        ApplicationDbContext dbContext;
        IBillingService billingService;
        IStringLocalizer<Yavsc.Resources.YavscLocalisation> localizer;

        public BillViewComponent(ApplicationDbContext dbContext, 
            IStringLocalizer<Yavsc.Resources.YavscLocalisation> localizer)
        {
            this.dbContext = dbContext;
            this.localizer = localizer;
        }
        /*
        public async Task<IViewComponentResult> InvokeAsync(string code, long id)
        {
            return await InvokeAsync(code, id, OutputFormat.Html);
        }
        public async Task<IViewComponentResult> InvokeAsync(string code, long id, OutputFormat outputFormat)
        {
            return await InvokeAsync(code,id,outputFormat,false,false);
        }
        public async Task<IViewComponentResult> InvokeAsync(string code, long id, OutputFormat outputFormat, bool asBill, bool acquitted)
        {
            var billable = await Task.Run( () => billingService.GetBillAsync(code,id));

            if (billable == null)
                throw new Exception("No data");
            return await InvokeAsync(code, billable, outputFormat,asBill,acquitted);

        } */

        public async Task<IViewComponentResult> InvokeAsync(string code, IBillable billable, OutputFormat format, bool asBill, bool acquitted)
        {
            var di = new DirectoryInfo(Startup.SiteSetup.UserFiles.Bills); 
            var dia = new DirectoryInfo(Startup.SiteSetup.UserFiles.Avatars); 
            ViewBag.BillsDir = di.FullName;
            ViewBag.AvatarsDir = dia.FullName;
            ViewBag.AsBill = asBill; // vrai pour une facture, sinon, c'est un devis
            ViewBag.Acquitted = acquitted;
            ViewBag.BillingCode = code;
            switch (format) {
                case OutputFormat.LaTeX :
                    var client = await dbContext.Users
                    .Include(u=>u.PostalAddress)
                    .SingleAsync(u=>u.Id == billable.ClientId);
                    ViewBag.Client = client;
                    var performer = await dbContext.Users
                    .Include(u=>u.BankInfo)
                    .Include(u=>u.PostalAddress)
                    .SingleAsync(u=>u.Id == billable.PerformerId);
                    ViewBag.Performer = performer;

                    ViewBag.ClientAddress = client.PostalAddress?.Address.SplitAddressToTeX();

                    var profile = await dbContext.Performers
                    .Include(p=>p.OrganizationAddress)
                    .SingleAsync(p=>p.PerformerId == billable.PerformerId);
                    ViewBag.PerformerProfile = profile;
                    ViewBag.ActivityLabel = localizer[billable.ActivityCode];
                    ViewBag.PerformerOrganizationAddress = profile.OrganizationAddress.Address.SplitAddressToTeX() ;
                    ViewBag.PerformerAddress = performer.PostalAddress?.Address.SplitAddressToTeX() ;
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
                            BaseFileName = $"bill-{code}-{billable.Id}"
                        } );
            }
            return View("Default",billable);
           
        }

    }
}
