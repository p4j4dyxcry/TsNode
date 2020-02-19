using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TsNode.Controls.Connection;
using TsNode.Controls.Drag;
using TsNode.Controls.Node;
using TsNode.Interface;

namespace TsNode.Controls
{
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_NodeItemsControl", Type = typeof(NodeItemsControl))]
    [TemplatePart(Name = "PART_ConnectionItemsControl", Type = typeof(ConnectionItemsControl))]
    [TemplatePart(Name = "PART_CreatingConnectionItemsControl", Type = typeof(ConnectionItemsControl))]
    public class NetworkView : Control , ITransformHolder
    {
        public static readonly DependencyProperty NodesProperty = DependencyProperty.Register(
            nameof(Nodes), typeof(IEnumerable<INodeDataContext>), typeof(NetworkView), new PropertyMetadata(default(IEnumerable<INodeDataContext>)));

        public IEnumerable<INodeDataContext> Nodes
        {
            get => (IEnumerable<INodeDataContext>) GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        public static readonly DependencyProperty ConnectionsProperty = DependencyProperty.Register(
            nameof(Connections), typeof(IEnumerable<IConnectionDataContext>), typeof(NetworkView), new PropertyMetadata(default(IEnumerable<IConnectionDataContext>)));

        public IEnumerable<IConnectionDataContext> Connections
        {
            get => (IEnumerable<IConnectionDataContext>) GetValue(ConnectionsProperty);
            set => SetValue(ConnectionsProperty, value);
        }

        public IEnumerable<NodeControl> SelectedItems => Enumerable.Range(0, _nodeItemsControl.Items.Count)
            .Select(x => _nodeItemsControl.FindAssociatedNodeItem(_nodeItemsControl.Items.GetItemAt(x)))
            .Where(x=>x.IsSelected);

        public static readonly DependencyProperty CanvasSizeProperty = DependencyProperty.Register(
            nameof(CanvasSize), typeof(double), typeof(NetworkView), new PropertyMetadata(default(double)));

        public double CanvasSize
        {
            get => (double) GetValue(CanvasSizeProperty);
            set => SetValue(CanvasSizeProperty, value);
        }

        public static readonly DependencyProperty GridSizeProperty = DependencyProperty.Register(
            nameof(GridSize), typeof(double), typeof(NetworkView), new PropertyMetadata(24.0d));

        public double GridSize
        {
            get => (double) GetValue(GridSizeProperty);
            set => SetValue(GridSizeProperty, value);
        }

        public static readonly DependencyProperty UseGridSnapProperty = DependencyProperty.Register(
            nameof(UseGridSnap), typeof(bool), typeof(NetworkView), new PropertyMetadata(true));

        public bool UseGridSnap
        {
            get => (bool) GetValue(UseGridSnapProperty);
            set => SetValue(UseGridSnapProperty, value);
        }

        public static readonly DependencyProperty CompletedCreateConnectionCommandProperty = DependencyProperty.Register(
            nameof(CompletedCreateConnectionCommand), typeof(ICommand), typeof(NetworkView), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty SelectionRectangleStyleProperty = DependencyProperty.Register(
            nameof(SelectionRectangleStyle), typeof(Style), typeof(NetworkView), new PropertyMetadata(default(Style)));

        public Style SelectionRectangleStyle
        {
            get => (Style) GetValue(SelectionRectangleStyleProperty);
            set => SetValue(SelectionRectangleStyleProperty, value);
        }

        //! コマンドの引数として[CompletedCreateConnectionEventArgs]が渡される
        public ICommand CompletedCreateConnectionCommand
        {
            get => (ICommand) GetValue(CompletedCreateConnectionCommandProperty);
            set => SetValue(CompletedCreateConnectionCommandProperty, value);
        }

        //! コマンドの引数として[StartCreateConnectionEventArgs]が渡される
        public static readonly DependencyProperty StartCreateConnectionCommandProperty = DependencyProperty.Register(
            nameof(StartCreateConnectionCommand), typeof(ICommand), typeof(NetworkView), new PropertyMetadata(default(ICommand)));

        public ICommand StartCreateConnectionCommand
        {
            get => (ICommand) GetValue(StartCreateConnectionCommandProperty);
            set => SetValue(StartCreateConnectionCommandProperty, value);
        }

        //! コマンドの引数として[CompletedMoveNodeEventArgs]が渡される
        public static readonly DependencyProperty CompetedMoveNodeCommandProperty = DependencyProperty.Register(
            nameof(CompetedMoveNodeCommand), typeof(ICommand), typeof(NetworkView), new PropertyMetadata(default(ICommand)));

        public ICommand CompetedMoveNodeCommand
        {
            get => (ICommand) GetValue(CompetedMoveNodeCommandProperty);
            set => SetValue(CompetedMoveNodeCommandProperty, value);
        }

        //! コマンドの引数として[SelectionChangedEventArgs]が渡される
        public static readonly DependencyProperty SelectionChangedCommandProperty = DependencyProperty.Register(
            nameof(SelectionChangedCommand), typeof(ICommand), typeof(NetworkView), new PropertyMetadata(default(ICommand)));

        public ICommand SelectionChangedCommand
        {
            get => (ICommand)GetValue(SelectionChangedCommandProperty);
            set => SetValue(SelectionChangedCommandProperty, value);
        }

        //! 
        private NodeItemsControl _nodeItemsControl;
        private ConnectionItemsControl _connectionItemsControl;
        private ConnectionItemsControl _creatingConnectionItemsControl;
        private Canvas _canvas;

        public ScaleTransform ScaleMatrix { get; } = new ScaleTransform(1, 1);
        public TranslateTransform TranslateMatrix { get; } = new TranslateTransform(0, 0);

        public void Initialize()
        {
            setup_drag_events();

            SetupWheel();
            SetupTestScrollBar();
        }

        private void setup_drag_events()
        {
            IDragController currentDragObject = null;            
            PreviewMouseDown += (s, e) =>
            {
                //! コントローラが処理中だった場合はキャンセルする
                currentDragObject?.Cancel();
                currentDragObject = null;

                if (_canvas.ContainChildren(e.OriginalSource as FrameworkElement) is false &&
                    (e.OriginalSource is ScrollViewer) is false)
                    return;
 
                //! ドラッグ開始時に適切なドラッグコントローラを作成
                currentDragObject = MakeDragController(e);
            };

            PreviewMouseMove += (s, e) =>
            {
                //! コントローラによるドラッグ処理を実施する
                currentDragObject?.OnDrag(s,e);
            };

            PreviewMouseUp += (s, e) =>
            {
                //! コントローラによるドラッグ処理を完了する
                currentDragObject?.DragEnd(s, e);
                currentDragObject = null;
            };
        }


        // セレクタを取得する/オーバーライドすることで選択処理を独自実装可能
        public virtual IControlSelector MakeControlSelector()
        {
            return new ControlSelector(SelectionChangedCommand);
        }

        public IDragController MakeDragController(MouseEventArgs args)
        {
            //! 左クリックに反応
            if (args.LeftButton == MouseButtonState.Pressed)
            {
                var nodes = _nodeItemsControl.GetNodes();
                var clickedNodes = _nodeItemsControl.GetNodes(x => x.IsMouseOver);
                var connections = _connectionItemsControl.GetConnectionShapes();

                // クリックしたコネクションを集める
                var clickedConnections = connections
                    .Where(x => x.HitTestCircle(args.GetPosition(_canvas), 12))
                    .ToArray();

                // 選択処理を行うセレクタを生成する
                IControlSelector selector = MakeControlSelector();

                var selectInfo = new SelectInfo(
                    nodes.ToSelectableDataContext(),
                    clickedNodes.ToSelectableDataContext(),
                    connections.ToSelectableDataContext(),
                    clickedConnections.ToSelectableDataContext());

                // 選択状態を設定する
                selector.OnSelect(selectInfo);                

                // ! ドラッグコントローラを作成する
                //   複雑な条件に対応できるように
                
                var builder = new DragControllerBuilder(args, this.FindChildWithName<Canvas>("PART_ItemsHost"), nodes, connections);
                return builder
                    .AddBuildTarget(new ConnectionDragBuild(builder, 0, _creatingConnectionItemsControl))
                    .AddBuildTarget(new NodeDragBuild(builder, 1, UseGridSnap, (int) GridSize))
                    .AddBuildTarget(new RectSelectionDragBuild(builder, 2, SelectionRectangleStyle))
                    .SetConnectionCommand(StartCreateConnectionCommand, CompletedCreateConnectionCommand)
                    .SetSelectionChangedCommand(SelectionChangedCommand)
                    .SetNodeDragControllerBuilder(CompetedMoveNodeCommand)
                    .Build();
            }

            if(args.MiddleButton == MouseButtonState.Pressed)
            {
                return new ViewportDrag(this,args);
            }
            
            // その他の場合はコントローラを作成しない ( つまりドラッグイベント無し )
            return null;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //! Find Controls
            _nodeItemsControl = this.FindTemplate<NodeItemsControl>("PART_NodeItemsControl");
            _canvas = this.FindTemplate<Canvas>("PART_Canvas");
            _connectionItemsControl = this.FindTemplate<ConnectionItemsControl>("PART_ConnectionItemsControl");
            _creatingConnectionItemsControl = this.FindTemplate<ConnectionItemsControl>("PART_CreatingConnectionItemsControl");
            //! Setup Events
            // ...

            Initialize();
        }

        // マウスホイール関係のテスト実装
        private void SetupWheel()
        {
            double scale = 1.0f;     // current scale 
            double minScale = 0.25f; 
            double maxScale = 4.0f;
            double scaleUnit = 1.2;

            var scaleTaeget = this.FindChildWithName<FrameworkElement>("PART_ItemsHost");

            var transformGroup = new TransformGroup();

            transformGroup.Children.Add(ScaleMatrix);
            transformGroup.Children.Add(TranslateMatrix);

            scaleTaeget.RenderTransform = transformGroup;

            PreviewMouseWheel += (s, e) =>
            {
                // スクロール
                if (Keyboard.IsKeyDown(Key.LeftCtrl) is false)
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.Right))
                    {
                        if (e.Delta > 0)
                            this.TranslateX(60);
                        else
                            this.TranslateX(-60);                                                
                    }
                    else
                    {
                        if (e.Delta > 0)
                            this.TranslateY(60);
                        else
                            this.TranslateY(-60);                        
                    }

                    return;
                }

                // スケーリング
                var delta = e.Delta < 0 ? 1.0 / scaleUnit : scaleUnit;
                scale = MathUtil.Clamp(scale * delta, minScale, maxScale);

                if (scaleTaeget != null)
                {
                    var mouse = Mouse.GetPosition(_canvas);
                    this.Scale(scale, mouse.X, mouse.Y);
                }
            };
        }

        private void SetupTestScrollBar()
        {
            // 試験実装 毎秒監視
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += (s, e) => UpdateScrollBar();
            timer.Start();
        }

        // TODO 整理
        private bool _lock = false;
        private bool _first = true;

        private double Snap(double value , double snap ,bool minus)
        {
            if(minus)
                return value - (value % snap) - snap;
            return value - (value % snap) + snap;
        }

        private ScrollBar _xSlider = null;
        private ScrollBar _ySlider = null;
        public void UpdateScrollBar()
        {
            int margin = 128;
            var minNodeX = this._nodeItemsControl.GetNodes().Min(x => x.X) - margin;
            var maxNodeX = this._nodeItemsControl.GetNodes().Max(x => x.X + x.ActualWidth) + margin;
            var minNodeY = this._nodeItemsControl.GetNodes().Min(x => x.Y) - margin;
            var maxNodeY = this._nodeItemsControl.GetNodes().Max(x => x.Y + x.ActualHeight) + margin;

            var left   = minNodeX * ScaleMatrix.ScaleX;
            var right  = maxNodeX * ScaleMatrix.ScaleX;
            var top    = minNodeY * ScaleMatrix.ScaleY;
            var bottom = maxNodeY * ScaleMatrix.ScaleY;
            _lock = true;

            if(_xSlider is null)
                _xSlider =  this.FindChildWithName<ScrollBar>("PART_XSlider");
            // TODO MoseDownではなくスクロールバードラッグ時に変更する
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                _xSlider.Minimum = Math.Min(left,TranslateMatrix.X);
                _xSlider.Maximum = Math.Max(right - ActualWidth,TranslateMatrix.X + ActualHeight);
                _xSlider.ViewportSize = ActualWidth;
            }
            _xSlider.Value = -TranslateMatrix.X;

            if (_xSlider.Maximum - _xSlider.Minimum <= 0)
                _xSlider.Visibility = Visibility.Hidden;
            else
                _xSlider.Visibility = Visibility.Visible;

            if(_ySlider is null)
                _ySlider =  this.FindChildWithName<ScrollBar>("PART_YSlider");

            var t = Mouse.LeftButton == MouseButtonState.Pressed ? Snap(-TranslateMatrix.Y, 200, true) : top;

            // TODO MoseDownではなくスクロールバードラッグ時に変更する
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                _ySlider.Minimum = Math.Min(top , -TranslateMatrix.Y);
                _ySlider.Maximum = Math.Max(bottom - ActualHeight , -TranslateMatrix.Y + ActualWidth);
                _ySlider.ViewportSize = ActualHeight;
            }
            _ySlider.Value = -TranslateMatrix.Y;

            if (_ySlider.Maximum - _ySlider.Minimum <= 0)
                _ySlider.Visibility = Visibility.Hidden;
            else
                _ySlider.Visibility = Visibility.Visible;

            _lock = false;
            if (_first)
            {
                _xSlider.ValueChanged += (s, e) =>
                {
                    if (_lock is false)
                        this.SetTranslateX(-_xSlider.Value);
                };
                
                _ySlider.ValueChanged += (s, e) =>
                {
                    if (_lock is false)
                        this.SetTranslateY(-_ySlider.Value);
                };
                _first = false;
            }
        }
    }
}
