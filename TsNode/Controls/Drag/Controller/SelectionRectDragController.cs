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
    public class SelectionRectDragControllerSetupArgs
    {
        public SelectionRectDragControllerSetupArgs(Panel baseControl, INodeControl[] nodes, ConnectionShape[] connections)
        {
            BaseControl = baseControl;
            Nodes = nodes;
            Connections = connections;
        }

        public Panel BaseControl { get; }
        public INodeControl[] Nodes { get; }
        public ConnectionShape[] Connections { get; }

        public Style RectangleStyle { get; set; }
        public ICommand SelectionChangedCommand { get; set; }
    }

    public class SelectionRectDragController : IDragController
    {
        private Rect _rect;

        private readonly Panel _panel;
        private readonly INodeControl[] _nodes;
        private readonly ConnectionShape[] _connections;
        private bool _canceled;

        private static readonly Rectangle Rectangle = new Rectangle();
        private static Style _defaultStyle ;
        private bool _mouseCaptured = false;
        private ICommand SelectionChangedCommand { get; }

        public SelectionRectDragController(SelectionRectDragControllerSetupArgs args)
        {
            _nodes = args.Nodes;
            _connections = args.Connections;
            _panel = args.BaseControl;
            SelectionChangedCommand = args.SelectionChangedCommand;

            //! オプション引数としてStyleが渡されている場合はStyleを適用する
            if (args.BaseControl.Style != null)
            {
               Debug.Assert(args.RectangleStyle.TargetType == typeof(Rectangle));
               Rectangle.Style = args.RectangleStyle;
            }
            else // デフォルトスタイルを利用する
            {
                if (_defaultStyle is null)
                    _defaultStyle = make_default_style();
                Rectangle.Style = _defaultStyle;
            }
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
            Canvas.SetLeft(Rectangle, args.StartPoint.X);
            Canvas.SetTop(Rectangle, args.StartPoint.Y);
            Rectangle.Width = 1;
            Rectangle.Height = 1;
            _panel.Children.Add(Rectangle);
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
                _mouseCaptured = _panel.CaptureMouse();
            
            var currentPoint = args.CurrentPoint;

            // 負のをUI座標には指定できないのでUI空間での座標系を再計算する
            {
                _rect.X = Math.Min(currentPoint.X, args.StartPoint.X);
                _rect.Y = Math.Min(currentPoint.Y, args.StartPoint.Y);
                _rect.Width = Math.Max(currentPoint.X, args.StartPoint.X) - _rect.X;
                _rect.Height = Math.Max(currentPoint.Y, args.StartPoint.Y) - _rect.Y;

                Canvas.SetLeft(Rectangle,_rect.X);
                Canvas.SetTop(Rectangle, _rect.Y);
                Rectangle.Width  = _rect.Width;
                Rectangle.Height = _rect.Height;
            }
        }

        public void OnDragEnd()
        {
            cancel_internal(true);
        }

        public void OnSelect()
        {
            var selectNodes = _panel.GetHitTestChildrenWithRect<NodeControl>(_rect).ToArray();

            if (selectNodes.Any())
            {
                var changed = SelectHelper.OnlySelect(_nodes.OfType<ISelectable>().Concat(_connections), selectNodes);
                SelectionChangedCommand?.Execute(new SelectionChangedEventArgs(changed));
                return;
            }
            var selectConnections = _connections.Where(x => x.HitTestRect(_rect)).ToArray();

            if (selectConnections.Any())
            {
                var changed = SelectHelper.OnlySelect(_nodes.OfType<ISelectable>().Concat(_connections), selectConnections);
                SelectionChangedCommand?.Execute(new SelectionChangedEventArgs(changed));
            }
        }

        private void cancel_internal(bool isSelect)
        {
            if (_canceled)
                return;

            _canceled = true;

            if (_panel.Children.Contains(Rectangle))
                _panel.Children.Remove(Rectangle);

            if(_mouseCaptured)
                this._panel.ReleaseMouseCapture();
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
