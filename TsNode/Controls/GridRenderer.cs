using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TsNode.Extensions;

namespace TsNode.Controls
{
    /// <summary>
    /// グリッド線レンダラ
    /// </summary>
    public class GridRenderer : Canvas
    {
        public static readonly DependencyProperty IsDrawGridProperty = DependencyProperty.Register(
            nameof(IsDrawGrid), typeof(bool), typeof(GridRenderer), new PropertyMetadata(true, OnDependencyPropertyChanged));

        public bool IsDrawGrid
        {
            get => (bool) GetValue(IsDrawGridProperty);
            set => SetValue(IsDrawGridProperty, value);
        }

        public static readonly DependencyProperty GridIntervalProperty = DependencyProperty.Register(
            nameof(GridInterval), typeof(double), typeof(GridRenderer), new PropertyMetadata(21.0D, OnDependencyPropertyChanged));

        public double GridInterval
        {
            get => (double) GetValue(GridIntervalProperty);
            set => SetValue(GridIntervalProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            nameof(Scale), typeof(double), typeof(GridRenderer), new PropertyMetadata(1.0D, OnDependencyPropertyChanged));

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public static readonly DependencyProperty GridBrushProperty = DependencyProperty.Register(
            nameof(GridBrush), typeof(Brush), typeof(GridRenderer), new PropertyMetadata(Brushes.Gray, OnDependencyPropertyChanged));

        public Brush GridBrush
        {
            get => (Brush) GetValue(GridBrushProperty);
            set => SetValue(GridBrushProperty, value);
        }

        public static readonly DependencyProperty GridThicknessProperty = DependencyProperty.Register(
            nameof(GridThickness), typeof(double), typeof(GridRenderer), new PropertyMetadata(0.1D, OnDependencyPropertyChanged));

        public double GridThickness
        {
            get => (double) GetValue(GridThicknessProperty);
            set => SetValue(GridThicknessProperty, value);
        }

        public static readonly DependencyProperty IsDashProperty = DependencyProperty.Register(
            nameof(IsDash), typeof(bool), typeof(GridRenderer), new PropertyMetadata(true, OnDependencyPropertyChanged));

        public bool IsDash
        {
            get => (bool)GetValue(IsDashProperty);
            set => SetValue(IsDashProperty, value);
        }

        public static readonly DependencyProperty DashAProperty = DependencyProperty.Register(
            nameof(DashA), typeof(double), typeof(GridRenderer), new PropertyMetadata(7D, OnDependencyPropertyChanged));

        public double DashA
        {
            get => (double) GetValue(DashAProperty);
            set => SetValue(DashAProperty, value);
        }

        public static readonly DependencyProperty DashBProperty = DependencyProperty.Register(
            nameof(DashB), typeof(double), typeof(GridRenderer), new PropertyMetadata(7D, OnDependencyPropertyChanged));

        public double DashB
        {
            get => (double) GetValue(DashBProperty);
            set => SetValue(DashBProperty, value);
        }

        private static void OnDependencyPropertyChanged( DependencyObject dependencyObject, 
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is GridRenderer gridCanvas)
                gridCanvas.make_pen();
        }

        private Pen _pen;

        // 背景テクスチャブラシの作成
        private void make_pen()
        {
            var width = double.IsNaN(Width)   ? ActualWidth  : Width;
            var height = double.IsNaN(Height) ? ActualHeight : Height;

            if (width <= 0 || height <= 0)
                return;


            var szX = GridInterval /  (width );
            var szY = GridInterval /  (height);

            var invScale = Scale >= 1.0 ? 1.0 / Scale : 1.0;
            if (IsDash)
            {
                var dashStyle = new DashStyle(new[] { DashA, DashB }, 0).DoFreeze();
                _pen = new Pen(GridBrush, GridThickness * invScale) { DashStyle = dashStyle }.DoFreeze();
            }
            else
            {
                _pen = new Pen(GridBrush, GridThickness * invScale).DoFreeze();
            }

            var geometry = new GeometryGroup();
            geometry.Children.Add(new LineGeometry(new Point(0, 0), new Point(8, 0)).DoFreeze());
            geometry.Children.Add(new LineGeometry(new Point(0, 0), new Point(0, 8)).DoFreeze());
            geometry.DoFreeze();

            var drawingBrush = new DrawingBrush()
            {
                TileMode = TileMode.Tile,
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0,0, szX , szY ),
                Drawing = new GeometryDrawing()
                {
                    Pen = _pen,
                    Geometry = geometry,                  
                }.DoFreeze()
            }.DoFreeze();

            Background = drawingBrush;
        }

        public GridRenderer()
        {
            //サイズが更新されたら背景テクスチャブラシを作り直す
            SizeChanged += (s, e) => make_pen();
        }

        public static double SnapTo(double a, double snap)
        {
            return Math.Max(a - (a % snap),snap);
        }
    }
}
