using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace TsControls.Behaviors
{
    public class SwipeScrollBehavior : Behavior<ScrollViewer>
    {
        #region IgnoreControlNames
        public static readonly DependencyProperty IgnoreControlNamesProperty =
            DependencyProperty.Register("IgnoreControlNames",
                typeof(IEnumerable<string>),
                typeof(SwipeScrollBehavior),
                new FrameworkPropertyMetadata(null));

        public IEnumerable<string> IgnoreControlNames
        {
            get => (IEnumerable<string>)GetValue(IgnoreControlNamesProperty);
            set => SetValue(IgnoreControlNamesProperty, value);
        }
        #endregion

        private ScrollViewer _scrollViewer;
        protected override void OnAttached()
        {
            base.OnAttached();

            var dragStart = DateTime.Now;
            var startPoint = new Point();
            var startDrag = false;
            var onMouseDown = false;
            var originStart = new Point();
            var onScroll = false;
            var mouseBuffer = new List<Point>();

            AssociatedObject.PreviewMouseWheel += (s, e) =>
            {
                if (onScroll)
                {
                    AssociatedObject.ReleaseMouseCapture();
                    onMouseDown = false;
                    onScroll = false;
                }
            };

            AssociatedObject. PreviewMouseDown += (s, e) =>
            {
                originStart = Mouse.GetPosition(AssociatedObject);
                mouseBuffer.Clear();
                mouseBuffer.Add(originStart);
                mouseBuffer.Add(originStart);
                mouseBuffer.Add(originStart);
                mouseBuffer.Add(originStart);
                mouseBuffer.Add(originStart);
                if (onScroll)
                {
                    AssociatedObject.ReleaseMouseCapture();
                    onMouseDown = false;
                    onScroll = false;
                }

                if (onMouseDown)
                    return;

                if(AssociatedObject.FindVisualChildrenWithType<ButtonBase>().Any(x=>x.IsMouseOver))
                    return;

                if (AssociatedObject.FindVisualChildrenWithType<Thumb>().Any(x => x.IsMouseOver))
                    return;

                if (IgnoreControlNames?.Any() is true)
                {
                    if (IgnoreControlNames
                        .Select(name => AssociatedObject.FindChildWithName<FrameworkElement>(name))
                        .Where(x=> x != null)
                        .Any(x => x.IsMouseOver))
                    {
                        return;
                    }
                }

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    onMouseDown = true;
                    dragStart = DateTime.Now;

                    e.Handled = true;
                }
            };

            Point get_avg_last()
            {
                var len = mouseBuffer.Count-1;

                var x = 0.0d;
                var y = 0.0d;

                var c = 5;
                var inv = 1.0d / 5.0d;
                for(int i=0; i<c; ++i)
                {
                    x += mouseBuffer[len - i].X * inv;
                    y += mouseBuffer[len - i].Y * inv;
                }

                return new Point(x, y);
            }

            AssociatedObject.PreviewMouseMove += (s, e) =>
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                    return;

                if (startDrag is false)
                {
                    if (onMouseDown is false)
                        return;

                    if (DateTime.Now - dragStart > TimeSpan.FromMilliseconds(30))
                    {
                        startPoint = Mouse.GetPosition(AssociatedObject);
                        AssociatedObject.CaptureMouse();
                        startDrag = true;
                    }
                    else
                    {
                        return;
                    }

                    if (_scrollViewer is null)
                        _scrollViewer = AssociatedObject as ScrollViewer ?? AssociatedObject.FindChildFirst<ScrollViewer>();

                }
                if (_scrollViewer is null)
                    return;

                startDrag = true;
                var vector = Mouse.GetPosition(AssociatedObject) - startPoint;
                vector *= -1;

                startPoint = Mouse.GetPosition(AssociatedObject);
                mouseBuffer.Add(startPoint);

                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + vector.Y);
                _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset + vector.X);
            };

            AssociatedObject.PreviewMouseUp += (s, e) =>
            {
                if (onMouseDown is false)
                    return;

                if (startDrag is false)
                {
                    onMouseDown = false;
                    return;
                }

                startDrag = false;
                var time = DateTime.Now - dragStart; // 時間
                var vector = Mouse.GetPosition(AssociatedObject) - get_avg_last(); //移動量
                vector *= -1;
                if (Math.Abs(vector.Y) > 10 || Math.Abs(vector.X) > 10)
                {
                    onScroll = true;
                    Task.Run(async () =>
                    {
                        const int  total = 100;
                        foreach (var x in Enumerable.Range(0, total))
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(20));

                            if (onScroll is false)
                                return;

                            Dispatcher.Invoke(() =>
                            {
                                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + vector.Y * ((total - x) * 0.05d) * 0.1d);
                                _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset + vector.X * ((total - x) * 0.05d) * 0.1d);
                            });
                        }

                        Dispatcher.Invoke(() =>
                        {
                            AssociatedObject.ReleaseMouseCapture();
                            onMouseDown = false;
                            onScroll = false;
                        });
                    });

                }
                else
                {
                    AssociatedObject.ReleaseMouseCapture();
                    onMouseDown = false;
                }
            };
        }
    }
}