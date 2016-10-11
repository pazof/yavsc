using BookAStar.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BookAStar.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var typeInfo = type.GetTypeInfo();
            var declaredMember = typeInfo.DeclaredMembers.FirstOrDefault(i => i.Name == value.ToString());
            var attribute = declaredMember?.GetCustomAttribute<DisplayAttribute>();

            return attribute == null ? value.ToString() : attribute.Description;
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
