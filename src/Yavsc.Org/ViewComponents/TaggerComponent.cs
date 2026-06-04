using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Yavsc.Interfaces;
using Yavsc.Models;

namespace Yavsc.ViewComponents
{
    public class TaggerViewComponent : ViewComponent
    {     
        readonly IStringLocalizer<TaggerViewComponent> localizer;
        public TaggerViewComponent(
            IStringLocalizer<TaggerViewComponent> pLocalizer)
        {
            this.localizer = pLocalizer;
        }
        public IViewComponentResult Invoke(ITaggable<long> longTaggable)
        {
            ViewBag.Tags = longTaggable.GetTags();
            ViewBag.at = localizer["at"];
            ViewBag.apictlr = "~/api/"+localizer["apiRouteTag"+longTaggable.GetType().Name];
            return View(longTaggable);
        }
    }
}
