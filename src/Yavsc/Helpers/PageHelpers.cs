using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Rendering;

namespace Yavsc.Server.Helpers
{
    public static class PageHelpers
    {
        public static List<SelectListItem> CreateSelectListItems (this Type enumType, object selectedValue =null)
        {
            string selectedName = (selectedValue != null) ? enumType.GetEnumName(selectedValue) : null;

            var items = new List<SelectListItem> ();
            var names = enumType.GetEnumNames();
            var values = enumType.GetEnumValues();
            for (int index = 0; index < names.Length; index++)
            {
                var itemName = names[index];
                items.Add(new SelectListItem() {
                Value = values.GetValue(index).ToString(), Text = itemName, Selected = ( itemName == selectedName)
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
