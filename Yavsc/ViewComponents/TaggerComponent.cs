using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Yavsc.Interfaces;
using Yavsc.Models;

namespace Yavsc.ViewComponents
{
    public class TaggerViewComponent : ViewComponent
    
    {
        ApplicationDbContext dbContext;
        IStringLocalizer<TaggerViewComponent> localizer;
        ILogger logger ;
        public TaggerViewComponent(
            ApplicationDbContext pdbContext,
            IStringLocalizer<TaggerViewComponent> pLocalizer,
            ILoggerFactory loggerFactory)
        {
            dbContext = pdbContext;
            this.localizer = pLocalizer;
            this.logger = loggerFactory.CreateLogger<TaggerViewComponent>();
        }
        public IViewComponentResult Invoke(ITaggable<long> longTaggable)
        {
            ViewData["Tags"] = longTaggable.GetTags();
            ViewData["at"] = localizer["at"];
            ViewData["apictlr"] = "~/api/"+localizer["apiRouteTag"+longTaggable.GetType().Name];
            return View(longTaggable);
        }
    }
}