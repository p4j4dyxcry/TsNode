
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using TsNode.Extensions;
using TsNode.Foundations;
using TsNode.Interface;

namespace TsNode.Controls
{
    public class InfiniteScrollViewer : ContentControl , ITransformHolder
    {
        public static readonly DependencyProperty ViewRectProperty = DependencyProperty.Register(
            "ViewRect", typeof(Rect), typeof(InfiniteScrollViewer), new PropertyMetadata(default(Rect),PropertyChangedCallback));
        public Rect ViewRect
        {
            get => (Rect) GetValue(ViewRectProperty);
            set => SetValue(ViewRectProperty, value);
        }

        public static readonly DependencyProperty ViewRectOffsetProperty = DependencyProperty.Register(
            "ViewRectOffset", typeof(Thickness), typeof(InfiniteScrollViewer), new PropertyMetadata(default(Thickness),PropertyChangedCallback));

        public Thickness ViewRectOffset
        {
            get => (Thickness) GetValue(ViewRectOffsetProperty);
            set => SetValue(ViewRectOffsetProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(1.0d));

        public double Scale
        {
            get => (double) GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register(
            "MinScale", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(0.25d));

        public double MinScale
        {
            get => (double) GetValue(MinScaleProperty);
            set => SetValue(MinScaleProperty, value);
        }

        public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register(
            "MaxScale", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(4.0d));

        public double MaxScale
        {
            get => (double) GetValue(MaxScaleProperty);
            set => SetValue(MaxScaleProperty, value);
        }

        public static readonly DependencyProperty ScaleUnitProperty = DependencyProperty.Register(
            "ScaleUnit", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(1.2d));

        public double ScaleUnit
        {
            get => (double) GetValue(ScaleUnitProperty);
            set => SetValue(ScaleUnitProperty, value);
        }

        public static readonly DependencyProperty TranslateUnitProperty = DependencyProperty.Register(
            "TranslateUnit", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(60d));

        public double TranslateUnit
        {
            get => (double) GetValue(TranslateUnitProperty);
            set => SetValue(TranslateUnitProperty, value);
        }

        public static readonly DependencyProperty ScrollRateProperty = DependencyProperty.Register(
            "ScrollRate", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(0.003d));

        public double ScrollRate
        {
            get => (double) GetValue(ScrollRateProperty);
            set => SetValue(ScrollRateProperty, value);
        }

        public static readonly DependencyProperty ScrollOffsetClampValueProperty = DependencyProperty.Register(
            "ScrollOffsetClampValue", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(300d));

        public double ScrollOffsetClampValue
        {
            get => (double) GetValue(ScrollOffsetClampValueProperty);
            set => SetValue(ScrollOffsetClampValueProperty, value);
        }
        
        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InfiniteScrollViewer infiniteScrollViewer)
            {
                infiniteScrollViewer.UpdateScrollBar();
            }
        }

        public ScaleTransform ScaleMatrix { get; } = new ScaleTransform(1, 1);
        public TranslateTransform TranslateMatrix { get; } = new TranslateTransform(0, 0);

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            setup_wheel();
            setup_size_change();
        }

        private bool _isUpdateFromThisControl ;
        private ScrollBar _xSlider ;
        private ScrollBar _ySlider ;

        public void UpdateScrollBar()
        {
            _isUpdateFromThisControl = true;

            if (setup_slider() is false)
            {
                return;
            }

            var rect = ViewRect
                .ToOffset(ViewRectOffset)
                .ToScale(ScaleMatrix.ScaleX , ScaleMatrix.ScaleY);
            
            _xSlider.Minimum = Math.Min(rect.Left,-TranslateMatrix.X);
            _xSlider.Maximum = Math.Max(rect.Right - ActualWidth, -TranslateMatrix.X);
            _xSlider.ViewportSize = ActualWidth;
            _xSlider.Value = -TranslateMatrix.X;

            _ySlider.Minimum = Math.Min(rect.Top , -TranslateMatrix.Y);
            _ySlider.Maximum = Math.Max(rect.Bottom  - ActualHeight , -TranslateMatrix.Y);
            _ySlider.ViewportSize = ActualHeight;
            _ySlider.Value = -TranslateMatrix.Y;

            update_slider_visibility(_xSlider);
            update_slider_visibility(_ySlider);
            
            _isUpdateFromThisControl = false;
        }

        public void FitRect(Rect fitRect)
        {
            var transformResult = TransformHolderHelper.ComputeFitRect(fitRect.ToOffset(ViewRectOffset), ActualWidth, ActualHeight);
            set_transform_origin(transformResult.X,transformResult.Y,transformResult.Scale);
        }

        /// <summary>
        /// 指定したRectにフィッティングさせます。
        /// </summary>
        /// <param name="fitRect"></param>
        /// <param name="time"></param>
        /// <param name="easing"></param>
        /// <returns></returns>
        public async Task FitRectAnimation(Rect fitRect, TimeSpan time , Func<double,double> easing = null)
        {
            var currentTransform = get_transform_origin();
            var targetTransform = TransformHolderHelper.ComputeFitRect(fitRect.ToOffset(ViewRectOffset), ActualWidth, ActualHeight);

            await EasingHelper.StartAnimation(t =>
            {
                set_transform_origin(
                    EasingHelper.Lerp(currentTransform.X, targetTransform.X, t),
                    EasingHelper.Lerp(currentTransform.Y, targetTransform.Y, t),
                    EasingHelper.Lerp(currentTransform.Scale, targetTransform.Scale, t));
            },time);
        }
        
        private void setup_wheel()
        {
            var scaleTarget = this.Content as FrameworkElement;
            
            if(scaleTarget is null)
                return;

            var transformGroup = new TransformGroup();

            transformGroup.Children.Add(ScaleMatrix);
            transformGroup.Children.Add(TranslateMatrix);

            scaleTarget.RenderTransform = transformGroup;

            MouseWheel += (s, e) =>
            {
                // スクロール
                if (Keyboard.IsKeyDown(Key.LeftCtrl) is false)
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.Right))
                    {
                        if (e.Delta > 0)
                            this.TranslateX(TranslateUnit);
                        else
                            this.TranslateX(-TranslateUnit);                                                
                    }
                    else
                    {
                        if (e.Delta > 0)
                            this.TranslateY(TranslateUnit);
                        else
                            this.TranslateY(-TranslateUnit);                        
                    }
                    UpdateScrollBar();
                    return;
                }

                // スケーリング
                var delta = e.Delta < 0 ? 1.0 / ScaleUnit : ScaleUnit;
                var newScale = MathExtensions.Clamp(Scale * delta, MinScale, MaxScale);
                SetValue(ScaleProperty,newScale);

                var mouse = Mouse.GetPosition(this);
                this.Scale(Scale, mouse.X, mouse.Y);
                UpdateScrollBar();
            };
        }
        
        private void setup_size_change()
        {
            this.SizeChanged += (s,e)=> UpdateScrollBar();
        }
        
        private void update_slider_visibility(ScrollBar slider)
        {
            if (slider.Maximum - slider.Minimum <= 0)
                slider.Visibility = Visibility.Hidden;
            else
                slider.Visibility = Visibility.Visible;
        }
        
        private void set_transform_origin(double x, double y, double scale)
        {
            var screenCenterPoint = new Point(ActualWidth/2, ActualHeight/2);
            
            // set translate
            {
                this.Scale(1, screenCenterPoint.X,screenCenterPoint.Y);
                this.SetTranslate(-x, -y);
                this.Translate(screenCenterPoint.X,screenCenterPoint.Y);
            }
            
            // set scale
            {
                this.SetValue(ScaleProperty,scale);
                this.Scale(scale, screenCenterPoint.X, screenCenterPoint.Y);                
            }
            UpdateScrollBar();
        }

        private TransformResult get_transform_origin()
        {
            var p = this.TransformPoint(ActualWidth / 2, ActualHeight / 2);
            var s = this.ScaleMatrix.ScaleX;
            return new TransformResult(p.X ,p.Y,s);
        }

        private bool setup_slider()
        {
            if (_xSlider is null)
            {
                _xSlider =  this.FindChildWithName<ScrollBar>("PART_XSlider");

                if (_xSlider is null)
                    return false;
                
                _xSlider.ValueChanged += (s, e) =>
                {
                    if (_isUpdateFromThisControl is false)
                        this.SetTranslateX(-_xSlider.Value);
                };
            }

            if (_ySlider is null)
            {
                _ySlider =  this.FindChildWithName<ScrollBar>("PART_YSlider");

                if (_ySlider is null)
                    return false;
                
                _ySlider.ValueChanged += (s, e) =>
                {
                    if (_isUpdateFromThisControl is false)
                        this.SetTranslateY(-_ySlider.Value);
                };
            }
            return true;
        }
    }
}