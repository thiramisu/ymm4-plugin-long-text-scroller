using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Data;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Settings;

namespace LongTextScrollerPlugin.Utils;

public class EnumDescriptionConverter : IValueConverter
{
    public static EnumDescriptionConverter Instance { get; } = new();
    public static object Convert(Enum enumValue)
    {
        var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());
        if (memberInfo.Length > 0)
        {
            var displayAttr = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
            if (displayAttr != null)
            {
                // ResourceType が指定されている場合はリソースから取得
                if (displayAttr.ResourceType != null)
                {
                    var resourceManager = new ResourceManager(displayAttr.ResourceType);
                    var resourceName = displayAttr.Name;
                    var culture = SettingsBase<YMMSettings>.Default.Language;

                    if (!string.IsNullOrEmpty(resourceName))
                    {
                        var resourceValue = culture == Language.Default
                            ? resourceManager.GetString(resourceName)
                            : resourceManager.GetString(resourceName, new CultureInfo((int)culture));
                        if (!string.IsNullOrEmpty(resourceValue))
                        {
                            return resourceValue;
                        }
                    }
                }

                // ResourceType がない場合、Name プロパティを直接使う
                if (!string.IsNullOrEmpty(displayAttr.Name))
                {
                    return enumValue.GetShortDisplayName();
                }
            }
        }

        // Display 属性が見つからない場合は enum の名前を返す
        return enumValue.ToString();

    }

    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? _) => (value is Enum enumValue) ? Convert(enumValue) : value?.ToString() ?? string.Empty;


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
