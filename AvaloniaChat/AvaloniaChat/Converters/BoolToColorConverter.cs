using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AvaloniaChat.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isUser)
            {
                return isUser ? new SolidColorBrush(Color.Parse("#E3F6FF")) : new SolidColorBrush(Color.Parse("#F9F9F9"));
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 