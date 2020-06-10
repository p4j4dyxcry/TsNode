using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using TsNode.Extensions;

namespace TsNode.Preset.Controls
{
    /// <summary>
    /// 小数からテキストに変換するコンバーター
    /// そのままBindingすると 「.」入力がはじかれるのでそれを回避する
    /// 代わりに TextBox.StringFormat等が利用できなくなる。
    /// </summary>
    public abstract class DecimalNumberConverter<T> : IValueConverter
    {
        private T _latestValue;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is T tValue)
                return _latestValue = tValue;
            return default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                if (TryConvert(stringValue, out var result))
                    return result;
            }
            
            // パースに失敗したら前回の値を返す。
            return _latestValue;
        }

        protected abstract bool TryConvert(string @string, out T result);

    }

    // 型毎に特殊化
    // double , float , decimal
    public class FloatConverter : DecimalNumberConverter<float>
    {
        protected override bool TryConvert(string @string, out float result) => float.TryParse(@string, out result);
    }
    
    public class DoubleConverter : DecimalNumberConverter<double>
    {
        protected override bool TryConvert(string @string, out double result) => double.TryParse(@string, out result);
    }
    
    public class DecimalConverter : DecimalNumberConverter<decimal>
    {
        protected override bool TryConvert(string @string, out decimal result) => decimal.TryParse(@string, out result);
    }
    
    // xaml markup extensions
    public class FloatConverterExtension: MarkupExtension
    {
        public override object ProvideValue( IServiceProvider serviceProvider ) => new  FloatConverter();
    }
    public class DoubleConverterExtension: MarkupExtension
    {
        public override object ProvideValue( IServiceProvider serviceProvider ) => new  DoubleConverter();
    }
    public class DecimalConverterExtension: MarkupExtension
    {
        public override object ProvideValue( IServiceProvider serviceProvider ) => new  DecimalConverter();
    }

    /// <summary>
    /// 色からブラシに変換するコンバーター
    /// </summary>
    public class ColorToBrushConverter : IValueConverter
    {
        private readonly Dictionary<Color, Brush> _brushes = new Dictionary<Color, Brush>();
        private static readonly Brush Default = Brushes.Transparent;
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                if (_brushes.TryGetValue(color, out var result))
                {
                    return result;
                }

                var brush = new SolidColorBrush(color).DoFreeze();
                return _brushes[color] = brush;
            }

            return Default;
        }

        // ユースケースが無いので one way only
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // xaml markup extension
    public class ColorToBrushConverterExtension : MarkupExtension
    {
        private static readonly ColorToBrushConverter StaticInstance = new ColorToBrushConverter();
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return StaticInstance;
        }
    }
}