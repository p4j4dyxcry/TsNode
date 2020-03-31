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
using TsNode.Controls.Drag.Controller;
using TsNode.Controls.Node;
using TsNode.Extensions;
using TsNode.Foundations;
using TsNode.Interface;

namespace TsNode.Controls
{
    [TemplatePart(Name = "PART_ItemsHost", Type = typeof(Panel))]
    [TemplatePart(Name = "PART_NodeItemsControl", Type = typeof(NodeItemsControl))]
    [TemplatePart(Name = "PART_ConnectionItemsControl", Type = typeof(ConnectionItemsControl))]
    [TemplatePart(Name = "PART_CreatingConnectionItemsControl", Type = typeof(ConnectionItemsControl))]
    public class NetworkView : Control
    {
        public static readonly DependencyProperty NodesProperty = DependencyProperty.Register(
            nameof(Nodes), typeof(IEnumerable<INodeDataContext>), typeof(NetworkView),
            new PropertyMetadata(default(IEnumerable<INodeDataContext>)));

        public IEnumerable<INodeDataContext> Nodes
        {
            get => (IEnumerable<INodeDataContext>) GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        public static readonly DependencyProperty ConnectionsProperty = DependencyProperty.Register(
            nameof(Connections), typeof(IEnumerable<IConnectionDataContext>), typeof(NetworkView),
            new PropertyMetadata(default(IEnumerable<IConnectionDataContext>)));

        public IEnumerable<IConnectionDataContext> Connections
        {
            get => (IEnumerable<IConnectionDataContext>) GetValue(ConnectionsProperty);
            set => SetValue(ConnectionsProperty, value);
        }

        public IEnumerable<NodeControl> SelectedItems => Enumerable.Range(0, _nodeItemsControl.Items.Count)
            .Select(x => _nodeItemsControl.FindAssociatedNodeItem(_nodeItemsControl.Items.GetItemAt(x)))
            .Where(x => x.IsSelected);

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

        public static readonly DependencyProperty CompletedCreateConnectionCommandProperty =
            DependencyProperty.Register(
                nameof(CompletedCreateConnectionCommand), typeof(ICommand), typeof(NetworkView),
                new PropertyMetadata(default(ICommand)));

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
            get => (Rect) GetValue(ItemsRectProperty);
            set => SetValue(ItemsRectProperty, value);
        }

        //! コマンドの引数として[CompletedCreateConnectionEventArgs]が渡される
        public ICommand CompletedCreateConnectionCommand
        {
            get => (ICommand) GetValue(CompletedCreateConnectionCommandProperty);
            set => SetValue(CompletedCreateConnectionCommandProperty, value);
        }

        //! コマンドの引数として[StartCreateConnectionEventArgs]が渡される
        public static readonly DependencyProperty StartCreateConnectionCommandProperty = DependencyProperty.Register(
            nameof(StartCreateConnectionCommand), typeof(ICommand), typeof(NetworkView),
            new PropertyMetadata(default(ICommand)));

        public ICommand StartCreateConnectionCommand
        {
            get => (ICommand) GetValue(StartCreateConnectionCommandProperty);
            set => SetValue(StartCreateConnectionCommandProperty, value);
        }

        public static readonly DependencyProperty CanceledConnectionCommandProperty = DependencyProperty.Register(
            nameof(CanceledConnectionCommand), typeof(ICommand), typeof(NetworkView), new PropertyMetadata(default(ICommand)));

        public ICommand CanceledConnectionCommand
        {
            get => (ICommand) GetValue(CanceledConnectionCommandProperty);
            set => SetValue(CanceledConnectionCommandProperty, value);
        }

        //! コマンドの引数として[CompletedMoveNodeEventArgs]が渡される
        public static readonly DependencyProperty CompetedMoveNodeCommandProperty = DependencyProperty.Register(
            nameof(CompetedMoveNodeCommand), typeof(ICommand), typeof(NetworkView),
            new PropertyMetadata(default(ICommand)));

        public ICommand CompetedMoveNodeCommand
        {
            get => (ICommand) GetValue(CompetedMoveNodeCommandProperty);
            set => SetValue(CompetedMoveNodeCommandProperty, value);
        }

        //! コマンドの引数として[SelectionChangedEventArgs]が渡される
        public static readonly DependencyProperty SelectionChangedCommandProperty = DependencyProperty.Register(
            nameof(SelectionChangedCommand), typeof(ICommand), typeof(NetworkView),
            new PropertyMetadata(default(ICommand)));

        public ICommand SelectionChangedCommand
        {
            get => (ICommand) GetValue(SelectionChangedCommandProperty);
            set => SetValue(SelectionChangedCommandProperty, value);
        }

        //! properties
        private NodeItemsControl _nodeItemsControl;
        private ConnectionItemsControl _connectionItemsControl;
        private ConnectionItemsControl _creatingConnectionItemsControl;
        private Panel _itemsHost;
        private DragEventBinder _panelBinder;
        private DragEventBinder _rootBinder;

        public NetworkView()
        {
            Loaded += (s, e) => _loaded();
            Unloaded += (s, e) => _unloaded();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //! Find Controls
            _nodeItemsControl = this.FindTemplate<NodeItemsControl>("PART_NodeItemsControl");
            _itemsHost = this.FindTemplate<Panel>("PART_ItemsHost");
            _connectionItemsControl = this.FindTemplate<ConnectionItemsControl>("PART_ConnectionItemsControl");
            _creatingConnectionItemsControl =
                this.FindTemplate<ConnectionItemsControl>("PART_CreatingConnectionItemsControl");

            //! Setup Events
            // ...
        }

        // セレクタを取得する/オーバーライドすることで選択処理を独自実装可能
        protected virtual IControlSelector MakeControlSelector()
        {
            return new ControlSelector(SelectionChangedCommand);
        }

        private void _loaded()
        {
            if (is_setup_completed() is false)
                return;
            
            _panelBinder = new DragEventBinder(_itemsHost, make_panel_drag_controller, true);
            _panelBinder.PreviewDragStart += update_selection;
            
            _rootBinder = new DragEventBinder(this, make_root_middle_drag_controller, true, _panelBinder);

            _itemsHost.KeyDown += key_down;
            start_node_update_timer();
        }

        private void _unloaded()
        {
            if (is_setup_completed() is false)
                return;
            
            if(_panelBinder != null)
                _panelBinder.PreviewDragStart -= update_selection;

            _panelBinder?.Dispose();
            _rootBinder?.Dispose();

            _itemsHost.KeyDown -= key_down;

            stop_node_update_timer();
        }

        private async void key_down(object s, KeyEventArgs args)
        {
            if (_itemsHost.IsFocused is false)
                return;
            
            if (args.Key == Key.F)
            {
                var infiniteScrollViewer = this.FindChild<InfiniteScrollViewer>(x => true);

                if (infiniteScrollViewer is null)
                    return;

                var nodes = this._nodeItemsControl.GetNodes().ToArray();
                if (nodes.Any(x => x.IsSelected))
                    nodes = nodes.Where(x => x.IsSelected).ToArray();
                var rect = compute_node_rect(nodes);
                args.Handled = true;

                await infiniteScrollViewer.FitRectAnimation(rect, TimeSpan.FromMilliseconds(200));
            }
        }

        private void update_selection(object sender , DragControllerEventArgs args)
        {
            // ミドルクリックは選択を変更しない
            if (args.Button == MouseButton.Middle)
                return;
            
            var nodes = _nodeItemsControl.GetNodes();
            var connections = _connectionItemsControl.GetConnectionShapes();
            var clickedNodes = _nodeItemsControl.GetNodes(x => x.IsMouseOver);

            // クリックしたコネクションを集める
            var clickedConnections = connections
                .Where(x => x.HitTestCircle(args.CurrentPoint, 12))
                .ToArray();

            // 選択処理を行うセレクタを生成する
            var selector = MakeControlSelector();

            var selectInfo = new SelectInfo(
                nodes.ToSelectableDataContext(),
                clickedNodes.ToSelectableDataContext(),
                connections.ToSelectableDataContext(),
                clickedConnections.ToSelectableDataContext());

            // 選択状態を設定する
            selector.OnSelect(selectInfo);
        }

        private IDragController make_panel_drag_controller(DragControllerEventArgs args)
        {
            //! 左クリックの場合ドラッグコントローラの作成を試みる
            if (args.Button == MouseButton.Left)
            {
                var nodes = _nodeItemsControl.GetNodes();
                var connections = _connectionItemsControl.GetConnectionShapes();
                
                // ! ドラッグコントローラを作成する
                var builder = new DragControllerBuilder(_itemsHost, MouseButton.Left, Keyboard.Modifiers , nodes, connections);
                return builder
                    .AddBuildTarget(new ConnectionDragBuild(builder, 0, _creatingConnectionItemsControl))
                    .AddBuildTarget(new NodesDragBuild(builder, 1, UseGridSnap, (int) GridSize)
                    {
                        ScrollViewer = this.FindChild<InfiniteScrollViewer>()
                    })
                    .AddBuildTarget(new RectSelectionDragBuild(builder, 2, SelectionRectangleStyle,_itemsHost))
                    .SetConnectionCommand(StartCreateConnectionCommand, CompletedCreateConnectionCommand , CanceledConnectionCommand)
                    .SetSelectionChangedCommand(SelectionChangedCommand)
                    .SetNodeDragCompletedCommand(CompetedMoveNodeCommand)
                    .Build();
            }

            // その他の場合はコントローラを作成しない ( つまりドラッグイベント無し )
            return null;
        }

        private IDragController make_root_middle_drag_controller(DragControllerEventArgs args)
        {
            if (args.Button == MouseButton.Middle)
            {
                return new ViewportDragController(this);
            }

            return null;
        }

        private DispatcherTimer _nodeRectCalcTimer;

        private void start_node_update_timer()
        {
            if (is_setup_completed() is false)
                return;
        
            if (_nodeRectCalcTimer is null)
            {
                _nodeRectCalcTimer = new DispatcherTimer();
                // 試験実装 毎秒監視
                _nodeRectCalcTimer.Interval = TimeSpan.FromSeconds(1.0);
                _nodeRectCalcTimer.Tick += (s, e) =>
                {
                    var nodes = this._nodeItemsControl.GetNodes();

                    if (nodes.Length == 0)
                        return;

                    SetValue(ItemsRectProperty, compute_node_rect(nodes));
                };
            }

            _nodeRectCalcTimer.Start();
        }

        private void stop_node_update_timer()
        {
            _nodeRectCalcTimer?.Stop();
        }

        private Rect compute_node_rect(IEnumerable<INodeControl> nodes)
        {
            var nodeControls = nodes as INodeControl[] ?? nodes.ToArray();
            var rect = new Rect()
            {
                X = nodeControls.Min(x => x.X),
                Y = nodeControls.Min(x => x.Y),
            };
            rect.Width = nodeControls.Max(x => x.X + x.ActualWidth) - rect.X;
            rect.Height = nodeControls.Max(x => x.Y + x.ActualHeight) - rect.Y;

            return rect;
        }

        private bool is_setup_completed()
        {
            if (_itemsHost is null)
                return false;
            if (_nodeItemsControl is null)
                return false;
            if (_connectionItemsControl is null)
                return false;
            if (_creatingConnectionItemsControl is null)
                return false;
            return true;
        }
    }
}