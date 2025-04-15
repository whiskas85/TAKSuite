using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BlazorReflection
{
    public static class Helpers
    {
        public static T? GetAttribute<T>(this PropertyInfo property) where T : Attribute
        {
            return property.GetCustomAttribute<T>();
        }

        public static string? GetDisplayName(this PropertyInfo property)
        {
            return property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
        }
        public static string GetDisplayName(this Enum enumValue)
        {
            var member = enumValue.GetType()
                                  .GetMember(enumValue.ToString())
                                  .FirstOrDefault();

            return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? enumValue.ToString();
        }
    }
}
