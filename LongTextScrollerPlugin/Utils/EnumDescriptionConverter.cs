using System.Globalization;
using System.Windows.Data;

namespace LongTextScrollerPlugin.Utils;

public class EnumDescriptionConverter : IValueConverter
{
    public static EnumDescriptionConverter Instance { get; } = new();

    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? _) => (value is Enum enumValue) ? EnumExtensions.Convert(enumValue) : value?.ToString() ?? string.Empty;


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
