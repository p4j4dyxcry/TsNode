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

        public void Initialize()
        {
            setup_drag_events();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += (s,e)=> UpdateCanvasSize();
            timer.Start();
        }

        private void setup_drag_events()
        {
            IDragController currentDragObject = null;
            double scale = 1.0f;
            double maxScale = 4.0f;
            double minScale = 0.25f;

            double oneDelta = 1.2;

            var scaleTaeget = this.FindChildWithName<FrameworkElement>("PART_ItemsHost");

            var transformGroup = new TransformGroup();

            transformGroup.Children.Add(ScaleMatrix);
            transformGroup.Children.Add(TranslateMatrix);

            scaleTaeget.RenderTransform = transformGroup; 

            PreviewMouseWheel += (s, e) =>
            {
                var delta = e.Delta < 0 ? 1.0 / oneDelta : oneDelta;

                if (Keyboard.IsKeyDown(Key.LeftCtrl) is false)
                {

                    return;
                }

                scale *= delta;

                if (scale >= maxScale)
                    scale = maxScale;

                if (scale <= minScale)
                    scale = minScale;

                if (scaleTaeget != null)
                {
                    var mouse = Mouse.GetPosition(_canvas);
                    this.Scale(scale, mouse.X, mouse.Y);
                    this.GridUpdate();
                    //GridUpdate();
                }
            };

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
                return new ViewportDrag(this,args,_canvas);
            }
            
            // その他の場合はコントローラを作成しない ( つまりドラッグイベント無し )
            return null;
        }

        Slider _xSlider = null;
        Slider _ySlider = null;
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
        public Point ViewportOffset { get; set; } = new Point(0, 0);

        public ScaleTransform ScaleMatrix { get; } = new ScaleTransform(1,1);

        public TranslateTransform TranslateMatrix { get; } = new TranslateTransform(0,0);

        public double MinNodeX;
        public double MaxNodeX;
        public double MinNodeY;
        public double MaxNodeY;
        public double CanvasLeft;
        public double CanvasTop;
        public double CanvasBottom;
        public double CanvasRight;

        private int hash = 0;

        public void UpdateCanvasSize()
        {
            MinNodeX = this.Nodes.Min(x => x.X);
            MaxNodeX = this.Nodes.Max(x => x.X) + 100;
            MinNodeY = this.Nodes.Min(x => x.Y);
            MaxNodeY = this.Nodes.Max(x => x.Y) + 100;

            var _gridRender = this.FindVisualChildrenWithType<GridRenderer>().FirstOrDefault();

            CanvasLeft = MinNodeX;
            CanvasRight = MaxNodeX;
            CanvasTop = MinNodeY;
            CanvasBottom = MaxNodeY;

            var canvasW = CanvasRight - CanvasLeft;
            var canvasH = CanvasBottom - CanvasTop;

            var h = CanvasLeft.GetHashCode() + CanvasBottom.GetHashCode() + CanvasTop.GetHashCode() + CanvasRight.GetHashCode();

            if (h == hash)
                return;
            hash = h;

            _gridRender = this.FindVisualChildrenWithType<GridRenderer>().FirstOrDefault();
            var scaleTaeget = this.FindChildWithName<FrameworkElement>("PART_ItemsHost");
            _gridRender.Scale = scaleTaeget.RenderTransform.Value.M11;

            _xSlider =  this.FindChildWithName<Slider>("PART_XSlider");
            _xSlider.Minimum = CanvasLeft;
            _xSlider.Maximum = CanvasRight;

            _xSlider.ValueChanged += (s, e) =>
            {
                this.SetTranslateX(-_xSlider.Value);
            };
            _ySlider =  this.FindChildWithName<Slider>("PART_YSlider");
            _ySlider.Minimum = CanvasTop;
            _ySlider.Maximum = CanvasBottom;

            _ySlider.ValueChanged += (s, e) =>
            {
                this.SetTranslateY(-_ySlider.Value);
            };
            UdateSliderValue();
        }

        private void UdateSliderValue()
        {
            _ySlider.Value = ViewportOffset.Y;
            _xSlider.Value = -ViewportOffset.X;
        }

        public double snapTo(double a , double snap)
        {
            if( a <= 0)
                return a - (a % snap) - snap;
            
            return a - (a % snap) + snap;
        }

        public void GridUpdate()
        {
            var _gridRender = this.FindVisualChildrenWithType<GridRenderer>().FirstOrDefault();
            if (_gridRender != null)
            {
                var scaleTaeget = this.FindChildWithName<FrameworkElement>("PART_ItemsHost");
                _gridRender.Scale = scaleTaeget.RenderTransform.Value.M11;
                UdateSliderValue();
            }
        }

    }
}
