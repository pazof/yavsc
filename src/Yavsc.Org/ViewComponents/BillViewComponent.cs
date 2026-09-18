
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Yavsc.Billing;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.ViewModels;
using Yavsc.ViewModels.Gen;
using Yavsc.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Yavsc.ViewComponents
{
    public class BillViewComponent : ViewComponent
    {
        readonly ApplicationDbContext dbContext;
        readonly IBillingService billing;
        readonly IStringLocalizer<BillViewComponent> localizer;
        private readonly SiteSettings siteSettings;

        public BillViewComponent(ApplicationDbContext dbContext,
            IStringLocalizer<BillViewComponent> localizer,
            IBillingService billing,
            IOptions<SiteSettings> siteSettings)
        {
            this.billing = billing;
            this.dbContext = dbContext;
            this.localizer = localizer;
            this.siteSettings = siteSettings.Value;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            string code,
            IBillable billable,
            OutputFormat format,
            bool asBill,
            SiteSettings settings
        )
        {
            var di = new DirectoryInfo(settings.Bills);
            var dia = new DirectoryInfo(settings.Avatars);
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

            var proAddr = profile.OrganizationAddress.Address;
            ViewBag.PerformerOrganizationAddress = proAddr.SplitAddressToTeX() ;
            ViewBag.FooterPerformerOrganizationAddress = proAddr.SplitAddressToTeX(", ");

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

                    var pdfGenerationViewModel = new PdfGenerationViewModel
                    {
                            Temp = siteSettings.TempDir,
                            TeXSource = tex,
                            DestDir = siteSettings.Bills,
                            BaseFileName = billable.GetFileBaseName(billing)
                        };
                    if (settings.GenerateEstimatePdf(pdfGenerationViewModel)) {
                        return this.View(new { Generated = pdfGenerationViewModel.BaseFileName+".pdf" });
                    } else {
                        return View(new { Error = pdfGenerationViewModel.GenerationErrorMessage } );
                    }
            }
            ViewBag.BillFileInfo =  billable.GetBillInfo(billing, siteSettings);
            ViewBag.Settings = settings;
            return View("Default",billable);

        }

    }
}
