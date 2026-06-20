using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;

namespace Yavsc.Server.Helpers
{
    public static class PageHelpers
    {
        /// <summary>
        /// Returns "active" aria-current="page" when the current route matches the given
        /// controller/action, otherwise an empty string. Use as an attribute value
        /// inside a &lt;a class="nav-link @..."&gt; element.
        /// Pass an empty controller (or "Home" with action "Index") to match the site root.
        /// </summary>
        public static IHtmlContent ActivePage(this ViewContext viewContext, string controller, string action = null)
        {
            var route = viewContext?.RouteData.Values;
            if (route == null) return HtmlString.Empty;

            var currentController = route["controller"] as string;
            var currentAction = route["action"] as string;

            // Normalize: empty or "Home" + ("Index" or null) both target the site root.
            bool isHome = string.IsNullOrEmpty(controller)
                || (string.Equals(controller, "Home", StringComparison.Ordinal)
                    && (action == null || string.Equals(action, "Index", StringComparison.Ordinal)));

            bool isCurrent;
            if (isHome)
            {
                isCurrent = string.IsNullOrEmpty(currentController)
                    || string.Equals(currentController, "Home", StringComparison.Ordinal)
                        && (currentAction == null || string.Equals(currentAction, "Index", StringComparison.Ordinal));
            }
            else
            {
                isCurrent = string.Equals(currentController, controller, StringComparison.Ordinal)
                    && (action == null || string.Equals(currentAction, action, StringComparison.Ordinal));
            }

            return isCurrent
                ? (IHtmlContent)new HtmlString("active\" aria-current=\"page")
                : HtmlString.Empty;
        }

        public static List<SelectListItem> CreateSelectListItems<T> (this IStringLocalizer<T>  localisation, Type enumType, object selectedValue =null)
        {
            string selectedName = (selectedValue != null) ? enumType.GetEnumName(selectedValue) : null;

            var items = new List<SelectListItem> ();
            var names = enumType.GetEnumNames();
            var values = enumType.GetEnumValues();
            for (int index = 0; index < names.Length; index++)
            {
                var itemName = names[index];
                items.Add(new SelectListItem() {
                Value = values.GetValue(index).ToString(), Text = localisation[itemName], Selected = ( itemName == selectedName)
                }) ;
            }
            var list = new SelectList(items);
            return items;
        }

        public static List<SelectListItem> AddNull(this List<SelectListItem> selectList, string displayNull, object selectedValue = null)
        {
          selectList.Add(new SelectListItem { Text = displayNull, Value = "", Selected = selectedValue == null });
          return selectList;
        }

        public static List<SelectListItem> CreateSelectListItems<T> (this IEnumerable<T>data,
         Func<T,string> dataField,
        Func<T,string> displayField = null, object selectedValue =null) where T : class
        {
            if (displayField == null) displayField = dataField;
             var items = new List<SelectListItem> ();
             foreach (var dataItem in data)
             {
                var itemVal = dataField(dataItem);
                var itemName = displayField(dataItem);
                items.Add(new SelectListItem() {
                Value = itemVal, Text = itemName, Selected = ( selectedValue?.Equals(itemVal) ?? false )
                }) ;
             }
             return items;
        }

    }
}
