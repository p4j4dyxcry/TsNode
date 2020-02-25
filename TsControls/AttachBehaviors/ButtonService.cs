using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TsControls.AttachBehaviors
{
    public static class ButtonService
    {
        /// <summary>
        /// ボタン角丸目の添付プロパティ
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(ButtonService),
            new PropertyMetadata(new CornerRadius(3, 3, 3, 3)));
        [AttachedPropertyBrowsableForType(typeof(ButtonBase))]
        public static object GetCornerRadius(DependencyObject d)
        {
            return d.GetValue(CornerRadiusProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(ButtonBase))]
        public static void SetCornerRadius(DependencyObject d, object value)
        {
            d.SetValue(CornerRadiusProperty, value);

            if (d is ButtonBase buttonBase)
            {
                var border = buttonBase.FindChildFirst<Border>();
                border?.SetCurrentValue(Border.CornerRadiusProperty , value);
            }
        }
    }
}