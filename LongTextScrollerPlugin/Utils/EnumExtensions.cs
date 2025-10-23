using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Settings;

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
    public static string ToLocalizedDisplayString(this Enum value)
    {
        var displayAttr = value.GetType().GetField(value.ToString())?.GetCustomAttribute<DisplayAttribute>();
        if (displayAttr is not null)
        {
            if (TryGetLocalizedDisplayStringFromResource(displayAttr, out var localizedDisplayString))
            {
                return localizedDisplayString;
            }

            var displayString = displayAttr.ShortName ?? displayAttr.Name;
            if (displayString is not null)
            {
                return displayString;
            }
        }

        // Display 属性からの取得ができない場合、enum の名前を返す
        return value.ToString();
    }

    static bool TryGetLocalizedDisplayStringFromResource(DisplayAttribute displayAttr, [MaybeNullWhen(false)] out string value)
    {
        value = null;

        var resourceType = displayAttr.ResourceType;
        if (resourceType is null)
        {
            return false;
        }

        var resourceName = displayAttr.Name;
        if (resourceName is null)
        {
            return false;
        }

        var resourceManager = new ResourceManager(resourceType);
        var culture = SettingsBase<YMMSettings>.Default.Language;
        var resourceValue = culture == Language.Default
            ? resourceManager.GetString(resourceName)
            : resourceManager.GetString(resourceName, new CultureInfo((int)culture));
        if (string.IsNullOrEmpty(resourceValue))
        {
            return false;
        }

        value = resourceValue;
        return true;
    }
}
