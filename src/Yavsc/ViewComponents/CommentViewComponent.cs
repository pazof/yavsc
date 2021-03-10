using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Localization;
using Yavsc.Models.Blog;

namespace Yavsc.ViewComponents
{
    public partial class CommentViewComponent : ViewComponent
    {
        private readonly IStringLocalizer<CommentViewComponent> localizer;
        public CommentViewComponent(IStringLocalizer<CommentViewComponent> localizer)
        {
            this.localizer = localizer;
        }
        public IViewComponentResult Invoke(IIdentified<long> longCommentable)
        {
            // for a BlogPost, it results in the localization 'apiRouteCommentBlogPost': blogcomments
            ViewData["apictlr"] = "/api/"+localizer["apiRouteComment"+longCommentable.GetType().Name];
            return View(longCommentable.GetType().Name, new Comment{ PostId = longCommentable.Id });
        }
    }
}
