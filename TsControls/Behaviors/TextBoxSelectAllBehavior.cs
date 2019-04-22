using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace TsControls.Behaviors
{
    public class TextBoxSelectAllBehavior
    {
        public static readonly DependencyProperty SelectAllOnGotFocusProperty =
            DependencyProperty.RegisterAttached(
                "SelectAllOnGotFocus",
                typeof(bool),
                typeof(TextBoxSelectAllBehavior),
                new UIPropertyMetadata(false, SelectAllOnGotFocusChanged)
            );

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static bool GetSelectAllOnGotFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectAllOnGotFocusProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetSelectAllOnGotFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectAllOnGotFocusProperty, value);
        }

        private static void SelectAllOnGotFocusChanged(DependencyObject sender, DependencyPropertyChangedEventArgs evt)
        {
            if (sender is TextBox textBox)
            {
                textBox.GotFocus -= OnTextBoxGotFocus;
                if ((bool)evt.NewValue)
                    textBox.GotFocus += OnTextBoxGotFocus;
            }
        }

        private static void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            Debug.Assert(textBox != null);

            textBox.Dispatcher.BeginInvoke((Action)(() => textBox.SelectAll()));
        }
    }
}
