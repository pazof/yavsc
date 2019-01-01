using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Localization;
using Yavsc.Models.Blog;

namespace Yavsc.ViewComponents
{
    public class CommentViewComponent : ViewComponent
    {
        IStringLocalizer<CommentViewComponent> localizer;
        public CommentViewComponent(IStringLocalizer<CommentViewComponent> localizer)
        {
           this.localizer = localizer;
        }
        public IViewComponentResult Invoke(IIdentified<long> longCommentable)
        {
            ViewData["apictlr"] = "/api/"+localizer["apiRouteComment"+longCommentable.GetType().Name];
            return View(longCommentable.GetType().Name, new Comment{ PostId = longCommentable.Id });
        }
    }
}