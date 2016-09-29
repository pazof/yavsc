using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BookAStar.Helpers
{
    public static class UserHelpers
    {
        public static ImageSource Avatar(string avatarPath)
        {
            /*foreach (var r in App.Current.Resources)
            {
                Debug.WriteLine($"#R# {r.Key} : {r.GetType().Name}");
            }*/
            var result = avatarPath == null ?
                ImageSource.FromResource(/* "BookAStar.icon_user.png" */ "BookAStar.Images.icon_user.png") :
                avatarPath.StartsWith("res://") ?
                 ImageSource.FromResource(avatarPath.Substring(6)) :
                 ImageSource.FromUri(new Uri(avatarPath));
            var test = ImageSource.FromResource("none.resource.png");
            return result;
        }
    }
}
