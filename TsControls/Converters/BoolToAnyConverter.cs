using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TsControls.Converters
{
    public class BoolToAnyConverter<T> : IValueConverter
    {
        public T True { get; set; }
        public T False { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? True : False;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value == (object)True;
    }

    public class BoolToIntConverter : BoolToAnyConverter<int> { }
    public class BoolToFloatConverter : BoolToAnyConverter<float> { }
    public class BoolToDoubleConverter : BoolToAnyConverter<double> { }
    public class BoolToStringConverter : BoolToAnyConverter<string> { }
    public class BoolToBrushConverter : BoolToAnyConverter<Brush> { }
    public class BoolToVisibilityConverter : BoolToAnyConverter<Visibility> { }
}
