using System;
using Microsoft.AspNet.Mvc.Rendering;
using Yavsc.Models.Drawing;

namespace Yavsc.Helpers
{
    public static class HtmlHelpers
    {
        public static HtmlString Color(this Color c)
        {
            if (c==null) return new HtmlString("#000");
             return new HtmlString(String.Format("#{0:X2}{1:X2}{2:X2}", c.Red, c.Green, c.Blue));
        }
    }
}