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
    public class NetworkView : Control
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

        public static readonly DependencyProperty ItemsRectProperty = DependencyProperty.Register(
            "ItemsRect", typeof(Rect), typeof(NetworkView), new PropertyMetadata(default(Rect)));

        public Rect ItemsRect
        {
            get { return (Rect) GetValue(ItemsRectProperty); }
            set { SetValue(ItemsRectProperty, value); }
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
        private void Initialize()
        {
            setup_drag_events();
            SetupUpdateRect();
        }

        private void setup_drag_events()
        {
            IDragController currentDragObject = null;            
            MouseDown += (s, e) =>
            {
                //! コントローラが処理中だった場合はキャンセルする
                currentDragObject?.Cancel();
                currentDragObject = null;
 
                //! ドラッグ開始時に適切なドラッグコントローラを作成
                currentDragObject = MakeDragController(e);
            };

            MouseMove += (s, e) =>
            {
                //! コントローラによるドラッグ処理を実施する
                currentDragObject?.OnDrag(s,e);
            };

            MouseUp += (s, e) =>
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

        private void SetupUpdateRect()
        {
            // 試験実装 毎秒監視
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1.0);
            timer.Tick += (s, e) =>
            {
                var nodes = this._nodeItemsControl.GetNodes();

                if (nodes.Length == 0)
                    return;
                
                var newRect = new Rect()
                {
                    X = nodes.Min(x=>x.X),
                    Y = nodes.Min(x=>x.Y),
                };
                newRect.Width = nodes.Max(x => x.X + x.ActualWidth)  - newRect.X;
                newRect.Height = nodes.Max(x => x.Y+ x.ActualHeight) - newRect.Y;
               
                SetValue(ItemsRectProperty,newRect);
            };
            timer.Start();
        }
    }
}
