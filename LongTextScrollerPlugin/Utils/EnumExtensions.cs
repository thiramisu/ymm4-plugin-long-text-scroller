using System.ComponentModel.DataAnnotations;

namespace LongTextScrollerPlugin.Utils;

public static class EnumExtensions
{
    public static string GetShortDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attr = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
                         .Cast<DisplayAttribute>()
                         .FirstOrDefault();
        return attr?.ShortName ?? attr?.Name ?? value.ToString();
    }
}
