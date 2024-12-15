using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Yavsc.Models;
using Yavsc.Models.Blog;

namespace Yavsc.ViewComponents
{
    public class CommentViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext context;
        private readonly IStringLocalizer<CommentViewComponent> localizer;
        public CommentViewComponent(IStringLocalizer<CommentViewComponent> localizer,
            ApplicationDbContext context
        )
        {
            this.context = context;
            this.localizer = localizer;
        }
        public async Task<IViewComponentResult> InvokeAsync(long id)
        {
            var comment = await context.Comment.Include(c=>c.Children).FirstOrDefaultAsync(c => c.Id==id);
            if (comment == null) 
                throw new InvalidOperationException();
            ViewData["apictlr"] = "/api/blogcomments";
            return View("Default", comment);
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
