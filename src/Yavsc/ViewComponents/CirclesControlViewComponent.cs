using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Yavsc.ViewComponents
{
    using Models;
    using ViewModels.Controls;
    using ViewModels.Relationship;
    using Yavsc.Abstract.Identity.Security;

    public class CirclesControlViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext dbContext;
        public CirclesControlViewComponent(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public IViewComponentResult Invoke (ICircleAuthorized target)
        {
            var oid = target.GetOwnerId();
            ViewBag.ACL = dbContext.Circle.Where(
                c=>c.OwnerId == oid)
                .Select(
                    c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = target.AuthorizeCircle(c.Id)
                    } 
                );

            ViewBag.Access = dbContext.Circle.Where(
                c=>c.OwnerId == oid)
                .Select( c=>
                    new AjaxCheckBoxInfo
                    {
                        Text = c.Name,
                        Checked  = target.AuthorizeCircle(c.Id),
                        Value = c.Id.ToString()
                    });
            return View(new CirclesViewModel(target));
        }
    }
}
