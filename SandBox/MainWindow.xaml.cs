using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TsNode.Controls;
using TsNode.Controls.Node;
using TsNode.Extensions;
using TsNode.Foundations;
using TsNode.Interface;
using TsNode.Preset;

namespace SandBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
         
            var nodeEngine = new NodeEngine();
            DataContext = nodeEngine.BuildViewModel();
            
            nodeEngine.Connect("Root","Child1");
            nodeEngine.Connect("Root","Child2");
            nodeEngine.Connect("Root","Child3");
            nodeEngine.Connect("Child2","Child4");
            nodeEngine.Connect("Child2","Child5");
            nodeEngine.Connect("Child3","Child6");
            nodeEngine.Connect("Child4","Child7");
            nodeEngine.Connect("Child7","Child8");
            
            // 自動配置のテスト
            nodeEngine.AutoArrange();
            
            // 配置されたノードにフィットさせる
            NetworkView.FitToSelectionNode(0);

            // ミニマップテスト
            CreateMinimap();

        }

        public async void CreateMinimap()
        {
            await Task.Delay(300);
            
            var brushes = new Brush[]
            {
                Brushes.RoyalBlue,
                Brushes.Coral,
                Brushes.Crimson,
                Brushes.HotPink,
                Brushes.LimeGreen,
                Brushes.BlueViolet,
            };


            int index = 0;
            Brush get_random_brush()
            {
                return brushes[(index++) % brushes.Length];
            }
            
            var timer = new DispatcherTimer();
            
            var canvas = new Canvas()
            {
                Background = Brushes.Gray,
                Opacity = 0.8,
            };
            
            Console.WriteLine(canvas.Width);
            Dictionary<INodeControl , Border> map = new Dictionary<INodeControl, Border>();

            var nodesControl = NetworkView.FindChild<NodeItemsControl>();
            var scrollViewer = this.FindChild<InfiniteScrollViewer>();
            var visualBrush = new VisualBrush();
            MiniMap.Background = visualBrush;

            var thumb = new Thumb()
            {
                Width = ActualWidth * scrollViewer.Scale ,
                Height = ActualHeight * scrollViewer.Scale,
                Background = Brushes.RoyalBlue,
                Opacity = 0.5,
                BorderBrush = Brushes.Yellow,
                BorderThickness = new Thickness(2)
            };

            Point start;
            Point translate;
            thumb.PreviewMouseDown += (s, e) =>
            {
                start = e.GetPosition(scrollViewer);
                translate = scrollViewer.GetTranslateToPosition();
                update_thumb(thumb, scrollViewer);
            };
            
            thumb.DragDelta += (s, e) =>
            {
                var rect = NetworkView.ItemsRect.ValidateRect(ActualWidth,ActualHeight)
                    .ToOffset(scrollViewer.ViewRectOffset);
                var p = Mouse.GetPosition(scrollViewer) - start;
                var x = e.HorizontalChange * (rect.Width / MiniMap.Width);
                var y = e.VerticalChange * (rect.Height / MiniMap.Height);

                x = MathExtensions.Clamp(x, rect.Left, rect.Right);
                y = MathExtensions.Clamp(y, rect.Left, rect.Right);
                
                scrollViewer.TranslateX(- x);
                scrollViewer.TranslateY(- y);

                update_thumb(thumb, scrollViewer);
            };
            MiniMap.Children.Add(thumb);
            
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += (s, e) =>
            {
                var rect = NetworkView.ItemsRect.ValidateRect(ActualWidth,ActualHeight).ToOffset(scrollViewer.ViewRectOffset);
                var point = scrollViewer.TransformPoint(0,0);
                thumb.Width  = ActualWidth    * (1.0 / scrollViewer.Scale);
                thumb.Height = ActualHeight   * (1.0 / scrollViewer.Scale);
                thumb.Width  = MiniMap.Width  * (thumb.Width / rect.Width);
                thumb.Height = MiniMap.Height * (thumb.Height / rect.Height);
                Canvas.SetLeft(thumb,point.X / (rect.Width / MiniMap.Width));
                Canvas.SetTop(thumb, point.Y/ (rect.Height / MiniMap.Height));
                canvas.Width = rect.Width;
                canvas.Height = rect.Height;

                foreach (var node in nodesControl.GetNodes())
                {
                    if (map.ContainsKey(node) is false)
                    {
                        map[node] = new Border()
                        {
                            Width = node.ActualWidth *  (rect.Width / rect.Height),
                            Height = node.ActualHeight *  (rect.Width / rect.Height),
                            Background = get_random_brush()
                        };                        
                    }

                    if (canvas.Children.Contains(map[node]) is false)
                    {
                        canvas.Children.Add(map[node]);
                    }

                    var converted_x = node.X - rect.Left;
                    
                    Canvas.SetLeft(map[node],converted_x);
                    Canvas.SetTop(map[node],node.Y - rect.Top);
                }

                foreach (var key_value in map)
                { 
                    if (nodesControl.Items.Contains(key_value.Key.DataContext) is false)
                        canvas.Children.Remove(key_value.Value);
                }
                visualBrush.Visual = canvas;
            };
            
            timer.Start();
        }

        private void update_thumb(Thumb thumb , InfiniteScrollViewer scrollViewer)
        {
            var rect = NetworkView.ItemsRect.ValidateRect(ActualWidth,ActualHeight).ToOffset(scrollViewer.ViewRectOffset);
            var point = scrollViewer.TransformPoint(0,0);
            Canvas.SetLeft(thumb,point.X / (rect.Width / MiniMap.Width));
            Canvas.SetTop(thumb, point.Y/ (rect.Height / MiniMap.Height));
        }
    }
}