using Microsoft.AspNet.Mvc;
using Yavsc.Helpers;
using Yavsc.Models.Billing;

namespace Yavsc.ViewComponents
{
    public class PayPalButtonViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(NominativeServiceCommand command, string apiControllerName , string controllerName)
        {
            ViewBag.CreatePaymentUrl = Request.ToAbsolute($"api/{apiControllerName}/createpayment/"+command.Id);
            ViewBag.ExecutePaymentUrl = Request.ToAbsolute("api/payment/execute");
            ViewBag.Urls=Request.GetPaymentUrls(controllerName,command.Id.ToString());
            return View ( command);
        }

    }
}
