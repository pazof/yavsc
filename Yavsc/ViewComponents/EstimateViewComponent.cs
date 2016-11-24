using System;
using System.IO;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.ViewModels.Gen;

namespace Yavsc.ViewComponents
{
    public class EstimateViewComponent : ViewComponent
    {
        ApplicationDbContext dbContext;
        public EstimateViewComponent(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public IViewComponentResult InvokeAsync(long id, bool toPdf = false)
        {
            Estimate estimate =
            dbContext.Estimates.Include(x => x.Query)
                   .Include(x => x.Query.Client)
                   .Include(x => x.Query.PerformerProfile)
                   .Include(x => x.Query.PerformerProfile.OrganizationAddress)
                   .Include(x => x.Query.PerformerProfile.Performer)
                   .Include(e => e.Bill).FirstOrDefault(x => x.Id == id);
            if (estimate == null)
                throw new Exception("No data");
            if (toPdf)
            {
                string tex = null;
                var oldWriter = ViewComponentContext.ViewContext.Writer;

                using (var writer = new StringWriter())
                {
                    this.ViewComponentContext.ViewContext.Writer = writer;

                    var resultTex = View("Estimate_tex", estimate);
                    resultTex.Execute(this.ViewComponentContext);
                    tex = writer.ToString();

                }
                ViewComponentContext.ViewContext.Writer = oldWriter;
                
                return this.View("Estimate_pdf", 
                        new PdfGenerationViewModel{ 
                            TeXSource = tex, 
                            DestDir = Startup.UserBillsDirName,
                            BaseFileName = $"estimate-{id}"
                        } );
            }
            return this.View("Estimate_tex", estimate);
        }

    }
}