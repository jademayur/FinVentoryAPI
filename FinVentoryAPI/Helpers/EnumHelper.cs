using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FinVentoryAPI.Helpers
{
    public static class EnumHelper
    {
        public static List<object> GetEnumList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new
                {
                    id = Convert.ToInt32(e),
                    name = e.ToString()
                })
                .Cast<object>()
                .ToList();
        }

        public static string GetDisplayName(Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()?
                .Name ?? enumValue.ToString();
        }
    }
}
