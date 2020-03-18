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

namespace TsNode.Controls.Drag
{
    public class SelectionRectDragControllerSetupArgs
    {
        public SelectionRectDragControllerSetupArgs(Panel baseControl, MouseEventArgs args, NodeControl[] nodes, ConnectionShape[] connections)
        {
            BaseControl = baseControl;
            Args = args;
            Nodes = nodes;
            Connections = connections;
        }

        public Panel BaseControl { get; }
        public MouseEventArgs Args { get; }

        public NodeControl[] Nodes { get; }
        public ConnectionShape[] Connections { get; }

        public Style RectangleStyle { get; set; }
        public ICommand SelectionChangedCommand { get; set; }
    }

    public class SelectionRectDragController : IDragController
    {
        private Point _startPoint;
        private Rect _rect;

        private readonly Panel _panel;
        private readonly NodeControl[] _nodes;
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


            var startPosition = args.Args.GetPosition(args.BaseControl);
            _startPoint.X = startPosition.X;
            _startPoint.Y = startPosition.Y;

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

            Canvas.SetLeft(Rectangle, _startPoint.X);
            Canvas.SetTop(Rectangle, _startPoint.Y);
            Rectangle.Width = 1;
            Rectangle.Height = 1;
            _panel.Children.Add(Rectangle);
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
            return true;
        }

        public void OnDrag(DragControllerEventArgs args)
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
                _rect.X = Math.Min(currentPoint.X, _startPoint.X);
                _rect.Y = Math.Min(currentPoint.Y, _startPoint.Y);
                _rect.Width = Math.Max(currentPoint.X, _startPoint.X) - _rect.X;
                _rect.Height = Math.Max(currentPoint.Y, _startPoint.Y) - _rect.Y;

                Canvas.SetLeft(Rectangle,_rect.X);
                Canvas.SetTop(Rectangle, _rect.Y);
                Rectangle.Width  = _rect.Width;
                Rectangle.Height = _rect.Height;
            }
        }

        public void DragEnd()
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
