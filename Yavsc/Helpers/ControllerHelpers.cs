using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
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

    }
}