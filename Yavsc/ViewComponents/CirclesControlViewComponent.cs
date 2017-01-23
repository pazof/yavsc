using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Yavsc.Models;
using Yavsc.ViewModels.Controls;
using Yavsc.ViewModels.Relationship;
using YavscLib;

namespace Yavsc.ViewComponents
{
    public class CirclesControlViewComponent : ViewComponent
    {
        ApplicationDbContext dbContext;
        public CirclesControlViewComponent(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<IViewComponentResult> InvokeAsync (ICircleAuthorized target)
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