using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TsNode.Controls.Connection;
using TsNode.Controls.Node;
using TsNode.Controls.Plug;
using TsNode.Interface;

namespace TsNode.Controls
{
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
            get { return (ICommand) GetValue(CompetedMoveNodeCommandProperty); }
            set { SetValue(CompetedMoveNodeCommandProperty, value); }
        }

        //! コマンドの引数として[SelectionChangedEventArgs]が渡される
        public static readonly DependencyProperty SelectionChangedCommandProperty = DependencyProperty.Register(
            nameof(SelectionChangedCommand), typeof(ICommand), typeof(NetworkView), new PropertyMetadata(default(ICommand)));

        public ICommand SelectionChangedCommand
        {
            get { return (ICommand)GetValue(SelectionChangedCommandProperty); }
            set { SetValue(SelectionChangedCommandProperty, value); }
        }


        //! 
        private NodeItemsControl _nodeItemsControl;
        private ConnectionItemsControl _connectionItemsControl;
        private ConnectionItemsControl _creatingConnectionItemsControl;
        private Canvas _canvas;

        public NetworkView()
        {
            Initialize();
        }

        public void Initialize()
        {
            setup_drag_events();
        }

        private void setup_drag_events()
        {
            IDragController currentDragObject = null;
            PreviewMouseDown += (s, e) =>
            {
                //! コントローラが処理中だった場合はキャンセルする
                currentDragObject?.Cancel();
 
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
            };;
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
                // NodeItemsControlsからすべてのノードを取得する
                var nodes = _nodeItemsControl
                    .EnumerateItems()
                    .Select(x => _nodeItemsControl.FindAssociatedNodeItem(x))
                    .ToArray();

                // 取得したノードからクリックしたノードを取得する
                var clickedNodes = nodes
                    .Where(x => x.IsMouseOver)
                    .ToArray();

                // ConnectionItemsControlからコネクションを集める
                var connections = _connectionItemsControl
                    .FindVisualChildrenWithType<ConnectionShape>()
                    .ToArray();

                // クリックしたコネクションを集める
                var clickedConnections = connections
                    .Where(x => x.HitTestCircle(args.GetPosition(_canvas),12))
                    .ToArray();

                // クリックしたノードの中でプラグがクリックされているものを集める
                var clickedPlugs = get_mouse_over_plugs(clickedNodes, out var sourcePlugType);

                // 選択処理を行うセレクタを生成する
                IControlSelector selector = MakeControlSelector();

                var selectInfo = new SelectInfo(
                    nodes.Select(x => x.DataContext).OfType<ISelectable>().ToArray(),
                    clickedNodes.Select(x => x.DataContext).OfType<ISelectable>().ToArray(),
                    connections.Select(x => x.DataContext).OfType<ISelectable>().ToArray(),
                    clickedConnections.Select(x => x.DataContext).OfType<ISelectable>().ToArray());

                // 選択状態を設定する
                selector.OnSelect(selectInfo);

                // プラグをクリックしている場合はConnectionCreateControllerを作成する
                if (clickedPlugs.Any())
                {
                    var setupArgs = new ConnectionCreateControllerSetupArgs(
                        _canvas,
                        args,
                        nodes,
                        clickedPlugs,
                        _creatingConnectionItemsControl,
                        CompletedCreateConnectionCommand,
                        StartCreateConnectionCommand,
                        sourcePlugType);

                    return new ConnectionCreateController(setupArgs);
                }

                var selectedNodes = nodes
                    .Where(x => x.IsSelected)
                    .ToArray();

                // ノードを選択している場合はNodeDragControllerを作成する
                if (selectedNodes.Any())
                {
                    var setupArgs = new NodeDragControllerSetupArgs(_canvas,
                                                                    args,
                                                                    selectedNodes,
                                                                    CompetedMoveNodeCommand)
                    {
                        GridSize = (int)this.GridSize,
                        UseSnapGrid = this.UseGridSnap,
                    };

                    return new NodeDragController(setupArgs);
                }

                // 選択に何もないので範囲選択を開始する
                {
                    var setupArgs = new SelectionRectDragControllerSetupArgs(_canvas,args,nodes,connections)
                    {
                        SelectionChangedCommand = SelectionChangedCommand,
                        RectangleStyle = SelectionRectangleStyle
                    };
                    return new SelectionRectDragController(setupArgs);
                }
            }

            // その他の場合はコントローラを作成しない ( つまりドラッグイベント無し )
            return null;
        }

        //クリックしたプラグ一覧を取得する(一覧と言っているが基本的には一つ)
        private PlugControl[] get_mouse_over_plugs(NodeControl[] nodes, out SourcePlugType sourcePlugType)
        {
            sourcePlugType = SourcePlugType.Output;
            var plugs = nodes
                .SelectMany(x => x.GetOutputPlugs())
                .Where(x => x.IsMouseOver)
                .ToArray();

            if (plugs.Length is 0)
            {
                sourcePlugType = SourcePlugType.Input;
                plugs = nodes
                    .SelectMany(x => x.GetInputPlugs())
                    .Where(x => x.IsMouseOver)
                    .ToArray();
            }

            return plugs;
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
        }
    }
}
