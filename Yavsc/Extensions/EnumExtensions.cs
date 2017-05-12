
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace Yavsc.Extensions
{
    public static class EnumExtensions
    {
        public static List<SelectListItem> GetSelectList (Type type, IStringLocalizer SR, Enum valueSelected)
        {
            var typeInfo = type.GetTypeInfo();
            var values = Enum.GetValues(type).Cast<Enum>();
            var items = new List<SelectListItem>();

            foreach (var value in values)
            {
                items.Add(new SelectListItem {
                    Text = SR[GetDescription(value, typeInfo)],
                    Value = value.ToString(),
                    Selected = value == valueSelected
                });
            }
            return items;
        }
        public static List<SelectListItem> GetSelectList (Type type, IStringLocalizer SR, string selectedValue = null)
        {
            var typeInfo = type.GetTypeInfo();
            var values = Enum.GetValues(type).Cast<Enum>();
            var items = new List<SelectListItem>();

            foreach (var value in values)
            {
                var strval = value.ToString();

                items.Add(new SelectListItem {
                     Text = SR[GetDescription(value, typeInfo)],
                     Value = strval,
                     Selected = strval == selectedValue
                });
            }
            return items;
        }
        public static string GetDescription(this Enum value, TypeInfo typeInfo )
        {
            var declaredMember = typeInfo.DeclaredMembers.FirstOrDefault(i => i.Name == value.ToString());
            var attribute = declaredMember?.GetCustomAttribute<DisplayAttribute>();
            return attribute == null ? value.ToString() : attribute.Description ?? attribute.Name;
        }
        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var typeInfo = type.GetTypeInfo();
            return GetDescription(value, typeInfo);
        }

        public static IEnumerable<string> GetDescriptions(Type type)
        {
            var values = Enum.GetValues(type).Cast<Enum>();
            var descriptions = new List<string>();

            foreach (var value in values)
            {
                descriptions.Add(value.GetDescription());
            }

            return descriptions;
        }

        public static Enum GetEnumFromDescription(string description, Type enumType)
        {
            var enumValues = Enum.GetValues(enumType).Cast<Enum>();
            var descriptionToEnum = enumValues.ToDictionary(k => k.GetDescription(), v => v);
            return descriptionToEnum[description];
        }
    }
}
