using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TsNode.Controls.Node;
using TsNode.Extensions;
using TsNode.Interface;

namespace SandBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Test();
        }

        public async void Test()
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
            var visualBrush = new VisualBrush();
            MiniMap.Background = visualBrush;
            
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += (s, e) =>
            {
                var rect = NetworkView.ItemsRect.ValidateRect(ActualWidth,ActualHeight);
                
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
                    
                    Console.WriteLine($"x = {node.X} converted = {converted_x}");
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
    }
}