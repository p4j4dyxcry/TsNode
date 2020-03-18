using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TsNode.Controls.Connection;
using TsNode.Controls.Node;
using TsNode.Extensions;
using TsNode.Foundations;
using TsNode.Interface;

namespace TsNode.Controls.Drag.Controller
{
    public class RectSelectionControllerSetupArgs
    {
        public RectSelectionControllerSetupArgs(INodeControl[] nodes, ConnectionShape[] connections , Panel panel = null)
        {
            Nodes = nodes;
            Connections = connections;
            Panel = panel;
        }

        public Panel Panel { get; }
        public INodeControl[] Nodes { get; }
        public ConnectionShape[] Connections { get; }

        public Style RectangleStyle { get; set; }
        public ICommand SelectionChangedCommand { get; set; }
    }

    public class RectSelectionController : IDragController
    {
        private Rect _rect;
        private RectSelectionControllerSetupArgs Args { get; }

        private bool _canceled;

        private Rectangle _rectangleView;
        private static Style _defaultStyle ;
        private bool _mouseCaptured = false;
        private ICommand SelectionChangedCommand { get; }

        public RectSelectionController(RectSelectionControllerSetupArgs args)
        {
            Args = args;
            SelectionChangedCommand = args.SelectionChangedCommand;
        }

        private Style make_default_style()
        {
            return new Style
            {
                TargetType = typeof(Rectangle),
                Setters =
                {
                    new Setter()
                    {
                        Property = Shape.StrokeProperty,
                        Value = Brushes.DeepSkyBlue,
                    },
                    new Setter()
                    {
                        Property = Shape.FillProperty,
                        Value = Brushes.LightSkyBlue,
                    },
                    new Setter()
                    {
                        Property = UIElement.OpacityProperty,
                        Value = 0.25d,
                    }
                }
            };
        }

        public bool CanDragStart(DragControllerEventArgs args)
        {
            return args.Button == MouseButton.Left;
        }
        
        public void OnStartDrag(DragControllerEventArgs args)
        {
            if (Args.Panel is null)
                return;

            _rectangleView = new Rectangle();
            
            //! オプション引数としてStyleが渡されている場合はStyleを適用する
            if (Args.RectangleStyle != null)
            {
                Debug.Assert(Args.RectangleStyle.TargetType == typeof(Rectangle));
                _rectangleView.Style = Args.RectangleStyle;
            }
            else // デフォルトスタイルを利用する
            {
                if (_defaultStyle is null)
                    _defaultStyle = make_default_style();
                _rectangleView.Style = _defaultStyle;
            }
            
            Canvas.SetLeft(_rectangleView, args.StartPoint.X);
            Canvas.SetTop(_rectangleView, args.StartPoint.Y);
            _rectangleView.Width = 1;
            _rectangleView.Height = 1;
            Args.Panel.Children.Add(_rectangleView);
        }

        public void OnDragMoving(DragControllerEventArgs args)
        {
            if (_canceled)
                return;

            if (args.Button == MouseButton.Right)
                cancel_internal(false);

            if (args.Button != MouseButton.Left)
                cancel_internal(true);

            if (_mouseCaptured is false)
                _mouseCaptured = Args.Panel.CaptureMouse();
            
            var currentPoint = args.CurrentPoint;

            // 負のをUI座標には指定できないのでUI空間での座標系を再計算する
            {
                _rect.X = Math.Min(currentPoint.X, args.StartPoint.X);
                _rect.Y = Math.Min(currentPoint.Y, args.StartPoint.Y);
                _rect.Width = Math.Max(currentPoint.X, args.StartPoint.X) - _rect.X;
                _rect.Height = Math.Max(currentPoint.Y, args.StartPoint.Y) - _rect.Y;

                if (_rectangleView != null)
                {
                    Canvas.SetLeft(_rectangleView,_rect.X);
                    Canvas.SetTop(_rectangleView, _rect.Y);
                    _rectangleView.Width  = _rect.Width;
                    _rectangleView.Height = _rect.Height;                    
                }
            }
        }

        public void OnDragEnd()
        {
            cancel_internal(true);
        }

        public void OnSelect()
        {
            var selectNodes = Args.Nodes.Where(x => x.ToRect().HitTest(_rect)).ToArray();

            if (selectNodes.Any())
            {
                var changed = SelectHelper.OnlySelect(Args.Nodes.OfType<ISelectable>().Concat(Args.Connections), selectNodes);
                SelectionChangedCommand?.Execute(new SelectionChangedEventArgs(changed));
                return;
            }

            if (Args.Connections.Any() is false)
                return;
            
            var selectConnections = Args.Connections.Where(x => x.HitTestRect(_rect)).ToArray();

            if (selectConnections.Any())
            {
                var changed = SelectHelper.OnlySelect(Args.Nodes.OfType<ISelectable>().Concat(Args.Connections), selectConnections);
                SelectionChangedCommand?.Execute(new SelectionChangedEventArgs(changed));
            }
        }

        private void cancel_internal(bool isSelect)
        {
            if (_canceled)
                return;

            _canceled = true;

            if (Args.Panel != null)
            {
                if (Args.Panel.Children.Contains(_rectangleView))
                    Args.Panel.Children.Remove(_rectangleView);

                if(_mouseCaptured)
                    Args.Panel.ReleaseMouseCapture();
            }
            
            _mouseCaptured = false;
            
            if (isSelect)
                OnSelect();
        }

        public void Cancel()
        {
            cancel_internal(true);
        }
    }
}
