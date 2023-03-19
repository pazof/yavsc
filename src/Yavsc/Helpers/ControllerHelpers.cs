using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Yavsc.Models.Messaging;

namespace Yavsc.Helpers
{
    public static class ControllerHelpers
    {
        public static void NotifyWarning(this Controller controller, string title, string body)
        {
            var notifs = SetupNotificationList(controller);
            notifs.Add(new Notification { title = title, body = body });
        }
        public static void NotifyInfo(this Controller controller, string title, string body)
        {
            var notifs = SetupNotificationList(controller);
            notifs.Add(new Notification { title = title, body = body });
        }
         public static void Notify(this Controller controller, IEnumerable<Notification> notes)
        {
            var notifs = SetupNotificationList(controller);
            notifs.AddRange(notes);
        }
        private static List<Notification> SetupNotificationList(this Controller controller)
        {
            List<Notification> notifs = (List<Notification>)controller.ViewData["Notify"];
            if (notifs == null)
            {
                controller.ViewData["Notify"] = notifs = new List<Notification>();
            }
            return notifs;
        }

        /// <summary>
        /// If Json is accepted, serve json, 
        /// if not, serve a web page.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static IActionResult ViewOk(this Controller controller, object model)
        {
            IActionResult result;
            if (JsonResponse(controller, model, out result)) return result;
            else return controller.View(model);
        }
        
        static bool JsonResponse(this Controller controller, object model, out IActionResult result){

            if (controller.Request.Headers.Keys.Contains("Accept")) {
                var accepted = controller.Request.Headers["Accept"];
                if (accepted == "application/json")
                {
                    if (controller.ModelState.ErrorCount>0) 
                       result = controller.BadRequest(controller.ModelState);
                    else 
                       result = controller.Ok(model);
                    return true;
                }
            }
            result = null;
            return false;
        }

        public static IActionResult ViewOk(this Controller controller, string viewname, object model = null)
        {
            IActionResult result;
            if (JsonResponse(controller, model, out result)) return result;
            else return controller.View(viewname, model);
        }
    }
}
