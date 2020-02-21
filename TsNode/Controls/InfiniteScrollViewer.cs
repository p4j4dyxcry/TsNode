
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using TsNode.Interface;

namespace TsNode.Controls
{
    public class InfiniteScrollViewer : ContentControl , ITransformHolder
    {
        public static readonly DependencyProperty ViewRectProperty = DependencyProperty.Register(
            "ViewRect", typeof(Rect), typeof(InfiniteScrollViewer), new PropertyMetadata(default(Rect),PropertyChangedCallback));
        public Rect ViewRect
        {
            get { return (Rect) GetValue(ViewRectProperty); }
            set { SetValue(ViewRectProperty, value); }
        }

        public static readonly DependencyProperty ViewRectOffsetProperty = DependencyProperty.Register(
            "ViewRectOffset", typeof(Thickness), typeof(InfiniteScrollViewer), new PropertyMetadata(default(Thickness),PropertyChangedCallback));

        public Thickness ViewRectOffset
        {
            get { return (Thickness) GetValue(ViewRectOffsetProperty); }
            set { SetValue(ViewRectOffsetProperty, value); }
        }

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(1.0d));

        public double Scale
        {
            get { return (double) GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register(
            "MinScale", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(0.25d));

        public double MinScale
        {
            get { return (double) GetValue(MinScaleProperty); }
            set { SetValue(MinScaleProperty, value); }
        }

        public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register(
            "MaxScale", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(4.0d));

        public double MaxScale
        {
            get { return (double) GetValue(MaxScaleProperty); }
            set { SetValue(MaxScaleProperty, value); }
        }

        public static readonly DependencyProperty ScaleUnitProperty = DependencyProperty.Register(
            "ScaleUnit", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(1.2d));

        public double ScaleUnit
        {
            get { return (double) GetValue(ScaleUnitProperty); }
            set { SetValue(ScaleUnitProperty, value); }
        }

        public static readonly DependencyProperty TranslateUnitProperty = DependencyProperty.Register(
            "TranslateUnit", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(60d));

        public double TranslateUnit
        {
            get { return (double) GetValue(TranslateUnitProperty); }
            set { SetValue(TranslateUnitProperty, value); }
        }

        public static readonly DependencyProperty ScrollRateProperty = DependencyProperty.Register(
            "ScrollRate", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(0.003d));

        public double ScrollRate
        {
            get { return (double) GetValue(ScrollRateProperty); }
            set { SetValue(ScrollRateProperty, value); }
        }

        public static readonly DependencyProperty ScrollOffsetClampValueProperty = DependencyProperty.Register(
            "ScrollOffsetClampValue", typeof(double), typeof(InfiniteScrollViewer), new PropertyMetadata(300d));

        public double ScrollOffsetClampValue
        {
            get { return (double) GetValue(ScrollOffsetClampValueProperty); }
            set { SetValue(ScrollOffsetClampValueProperty, value); }
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
            SetupWheel();
        }

        // マウスホイール関係のイベントバインド
        private void SetupWheel()
        {
            var scaleTarget = this.Content as FrameworkElement;
            
            if(scaleTarget is null)
                return;

            var transformGroup = new TransformGroup();

            transformGroup.Children.Add(ScaleMatrix);
            transformGroup.Children.Add(TranslateMatrix);

            scaleTarget.RenderTransform = transformGroup;

            PreviewMouseWheel += (s, e) =>
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
                var newScale = MathUtil.Clamp(Scale * delta, MinScale, MaxScale);
                SetValue(ScaleProperty,newScale);

                var mouse = Mouse.GetPosition(this);
                this.Scale(Scale, mouse.X, mouse.Y);
                UpdateScrollBar();
            };
        }
        
        private bool _isUpdateFromThisControl = false;

        private ScrollBar _xSlider = null;
        private ScrollBar _ySlider = null;
        public void UpdateScrollBar()
        {
            _isUpdateFromThisControl = true;
            
            var left   = (ViewRect.Left   - ViewRectOffset.Left)   * ScaleMatrix.ScaleX;
            var right  = (ViewRect.Right  + ViewRectOffset.Right)  * ScaleMatrix.ScaleX;
            var top    = (ViewRect.Top    - ViewRectOffset.Top)    * ScaleMatrix.ScaleY;
            var bottom = (ViewRect.Bottom + ViewRectOffset.Bottom) * ScaleMatrix.ScaleY;

            if (SetupSlider() is false)
            {
                return;
            }

            _xSlider.Minimum = Math.Min(left,-TranslateMatrix.X);
            _xSlider.Maximum = Math.Max(right - ActualWidth, -TranslateMatrix.X);
            _xSlider.ViewportSize = ActualWidth;
            _xSlider.Value = -TranslateMatrix.X;

            _ySlider.Minimum = Math.Min(top , -TranslateMatrix.Y);
            _ySlider.Maximum = Math.Max(bottom  - ActualWidth, -TranslateMatrix.Y );
            _ySlider.ViewportSize = ActualHeight;
            _ySlider.Value = -TranslateMatrix.Y;

            UpdateSliderVisible(_xSlider);
            UpdateSliderVisible(_ySlider);
            
            _isUpdateFromThisControl = false;
        }

        private void UpdateSliderVisible(ScrollBar slider)
        {
            if (slider.Maximum - _xSlider.Minimum <= 0)
                slider.Visibility = Visibility.Hidden;
            else
                slider.Visibility = Visibility.Visible;
        }

        private bool SetupSlider()
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