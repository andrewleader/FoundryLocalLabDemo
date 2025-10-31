using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FoundryLocalLabDemo;

/// <summary>
/// Converts boolean IsUser property to HorizontalAlignment for chat message positioning
/// </summary>
public class BoolToAlignmentConverter : IValueConverter
{
    public static readonly BoolToAlignmentConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isUser)
        {
            return isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }
        return HorizontalAlignment.Left;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean IsUser property to Color for chat message background
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public static readonly BoolToColorConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isUser)
        {
            return isUser ? Colors.LightGreen : Colors.LightBlue;
        }
        return Colors.LightBlue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean IsUser property to FontWeight
/// </summary>
public class BoolToFontWeightConverter : IValueConverter
{
    public static readonly BoolToFontWeightConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isUser)
        {
            return isUser ? FontWeights.Normal : FontWeights.Normal;
        }
        return FontWeights.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean to Visibility
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public static readonly BoolToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts model name and selected model name to boolean for radio button selection
/// </summary>
public class ModelSelectionConverter : IMultiValueConverter
{
    public static readonly ModelSelectionConverter Instance = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is string modelName && values[1] is string selectedModelName)
        {
            return string.Equals(modelName, selectedModelName, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}