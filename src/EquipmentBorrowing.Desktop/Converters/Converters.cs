using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace EquipmentBorrowing.Desktop.Converters;

public class BoolToAvailabilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isAvailable)
        {
            return isAvailable ? "Available" : "Borrowed";
        }
        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToAvailabilityColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isAvailable)
        {
            return isAvailable ? SolidColorBrush.Parse("#198754") : SolidColorBrush.Parse("#DC3545");
        }
        return SolidColorBrush.Parse("#6C757D");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToFeedbackBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSuccess)
        {
            return isSuccess ? SolidColorBrush.Parse("#D1E7DD") : SolidColorBrush.Parse("#F8D7DA");
        }
        return SolidColorBrush.Parse("#E2E3E5");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToFeedbackForegroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSuccess)
        {
            return isSuccess ? SolidColorBrush.Parse("#0F5132") : SolidColorBrush.Parse("#842029");
        }
        return SolidColorBrush.Parse("#41464B");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToFeedbackBorderConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSuccess)
        {
            return isSuccess ? SolidColorBrush.Parse("#BADBCC") : SolidColorBrush.Parse("#F5C2C7");
        }
        return SolidColorBrush.Parse("#D3D6D8");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
