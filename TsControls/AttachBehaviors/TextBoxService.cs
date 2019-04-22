using System.Windows;
using System.Windows.Controls;

namespace TsControls.AttachBehaviors
{
    class TextBoxService
    {
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(TextBoxService),
            new PropertyMetadata(new CornerRadius(0, 0, 0, 0), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox t)
            {
                var border = t.FindVisualParentWithType<Border>();
                if (border != null)
                    border.CornerRadius = (CornerRadius)e.NewValue;
            }
        }

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static object GetCornerRadius(DependencyObject d)
        {
            return d.GetValue(CornerRadiusProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetCornerRadius(DependencyObject d, object value)
        {
            d.SetValue(CornerRadiusProperty, value);
        }
    }
}
