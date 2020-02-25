using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace TsControls.AttachBehaviors
{
    public static class TextBoxWatermarkService
    {
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
            "Watermark",
            typeof(object),
            typeof(TextBoxWatermarkService),
            new FrameworkPropertyMetadata(null, OnWatermarkChanged));

        public static object GetWatermark(DependencyObject d)
        {
            return d.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject d, object value)
        {
            d.SetValue(WatermarkProperty, value);
        }

        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                control.Loaded += Control_Loaded;

                if (d is TextBox textBox)
                {
                    control.GotKeyboardFocus += Control_GotKeyboardFocus;
                    control.LostKeyboardFocus += Control_Loaded;
                    textBox.TextChanged += Control_GotKeyboardFocus;
                }
            }
        }

        private static void Control_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Control control)
                RemoveWatermark(control);
        }

        private static void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox control && ShouldShowWatermark(control))
                ShowWatermark(control);
        }

        private static void RemoveWatermark(UIElement control)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);

            foreach (var adorner in layer?.GetAdorners(control)?.ToArray() ?? new Adorner[0])
            {
                if (adorner is TextBoxWatermarkAdorner)
                {
                    adorner.Visibility = Visibility.Collapsed;
                    layer?.Remove(adorner);
                }
            }
        }

        private static async void ShowWatermark(TextBox control)
        {
            // コントロールのパラメータ解決が終わった後に実行したいので遅延させる
            await Task.Delay(1);

            if (AdornerLayer.GetAdornerLayer(control) is AdornerLayer layer)
            {
                layer.Add(new TextBoxWatermarkAdorner(control, GetWatermark(control)));
            }
        }

        private static bool ShouldShowWatermark(Control c)
        {
            if (c is TextBox textBox)
            {
                if (string.IsNullOrEmpty(textBox.Text) is false)
                    return false;

                if (textBox.IsFocused)
                    return true;

                return textBox.Text == string.Empty;
            }
            return false;
        }

        internal class TextBoxWatermarkAdorner : Adorner
        {
            private readonly ContentPresenter _contentPresenter;

            public TextBoxWatermarkAdorner(TextBox adornedElement, object watermark) : base(adornedElement)
            {
                IsHitTestVisible = false;

                if (watermark is string)
                {
                    watermark = new TextBlock()
                    {
                        Text = watermark.ToString(),
                        Foreground = adornedElement.Foreground,
                        FontSize = adornedElement.FontSize,
                        FontStretch = adornedElement.FontStretch,
                        FontFamily = adornedElement.FontFamily,
                        HorizontalAlignment = adornedElement.HorizontalContentAlignment,
                        Margin = adornedElement.Padding,
                    };
                }
                
                _contentPresenter = new ContentPresenter
                {
                    Content = watermark,
                    VerticalAlignment = VerticalAlignment.Center,
                    Opacity = 0.5,
                    Margin = new Thickness(0, 0, 0, 0),
                };

                var binding = new Binding(nameof(IsVisible))
                {
                    Source = adornedElement,
                    Converter = new BooleanToVisibilityConverter()
                };
                SetBinding(VisibilityProperty, binding);

                this.UpdateLayout();
            }

            protected override int VisualChildrenCount => 1;

            private Control Control => AdornedElement as Control;

            protected override Visual GetVisualChild(int index)
            {
                return _contentPresenter;
            }

            protected override Size MeasureOverride(Size constraint)
            {
                _contentPresenter.Measure(Control.RenderSize);
                return Control.RenderSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                _contentPresenter.Arrange(new Rect(finalSize));
                return finalSize;
            }
        }
    }
}
