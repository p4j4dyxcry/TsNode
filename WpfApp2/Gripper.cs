using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp2
{
    public class Gripper : Adorner
    {
        private readonly Thumb _resizeGrip;
        private readonly VisualCollection _visualChildren;

        public Gripper(UIElement adornedElement , ControlTemplate controlTemplate) : base(adornedElement)
        {
            _resizeGrip = new Thumb
            {
                Cursor = Cursors.SizeNWSE
            };
            _resizeGrip.SetValue(WidthProperty,18d);
            _resizeGrip.SetValue(HeightProperty, 18d);
            _resizeGrip.DragDelta += OnGripDelta;

            _resizeGrip.Template = controlTemplate ?? MakeDefaultGripTemplate();

            _visualChildren = new VisualCollection(this)
            {
                _resizeGrip
            };
        }

        private ControlTemplate MakeDefaultGripTemplate()
        {
            //! 指定なしの場合の見た目を作成
            var visualTree = new FrameworkElementFactory(typeof(Border));
            visualTree.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            visualTree.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            visualTree.SetValue(WidthProperty, 12d);
            visualTree.SetValue(HeightProperty, 12d);
            visualTree.SetValue(Border.BackgroundProperty, new SolidColorBrush(Colors.RoyalBlue));
            visualTree.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));

            return new ControlTemplate(typeof(Thumb))
            {
                VisualTree = visualTree
            };
        }

        private void OnGripDelta(object sender, DragDeltaEventArgs e)
        {
            if (AdornedElement is FrameworkElement frameworkElement)
            {
                var w = frameworkElement.Width;
                var h = frameworkElement.Height;
                if (w.Equals(double.NaN))
                    w = frameworkElement.DesiredSize.Width;
                if (h.Equals(double.NaN))
                    h = frameworkElement.DesiredSize.Height;

                w += e.HorizontalChange;
                h += e.VerticalChange;

                // clamp
                w = Math.Max(_resizeGrip.Width, w);
                h = Math.Max(_resizeGrip.Height, h);
                w = Math.Max(frameworkElement.MinWidth, w);
                h = Math.Max(frameworkElement.MinHeight, h);
                w = Math.Min(frameworkElement.MaxWidth, w);
                h = Math.Min(frameworkElement.MaxHeight, h);

                // ※ = で入れるとBindingが外れるので注意
                frameworkElement.SetValue(WidthProperty,w);
                frameworkElement.SetValue(HeightProperty,h);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (AdornedElement is FrameworkElement frameworkElement)
            {
                var w = _resizeGrip.Width;
                var h = _resizeGrip.Height;
                var x = frameworkElement.ActualWidth - w;
                var y = frameworkElement.ActualHeight - h;

                _resizeGrip.Arrange(new Rect(x, y, w, h));
            }
            return finalSize;
        }

        protected override int VisualChildrenCount => _visualChildren.Count;

        protected override Visual GetVisualChild(int index)
        {
            return _visualChildren[index];
        }
    }
}