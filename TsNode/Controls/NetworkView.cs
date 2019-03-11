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
            nameof(Nodes), typeof(IEnumerable<INodeViewModel>), typeof(NetworkView), new PropertyMetadata(default(IEnumerable<INodeViewModel>)));

        public IEnumerable<INodeViewModel> Nodes
        {
            get => (IEnumerable<INodeViewModel>) GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        public static readonly DependencyProperty ConnectionsProperty = DependencyProperty.Register(
            nameof(Connections), typeof(IEnumerable<IConnectionViewModel>), typeof(NetworkView), new PropertyMetadata(default(IEnumerable<IConnectionViewModel>)));

        public IEnumerable<IConnectionViewModel> Connections
        {
            get => (IEnumerable<IConnectionViewModel>) GetValue(ConnectionsProperty);
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

        public virtual IControlSelector MakeControlSelector()
        {
            return new ControlSelector(SelectionChangedCommand);
        }

        public IDragController MakeDragController(MouseEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Pressed)
            {
                var nodes = _nodeItemsControl
                    .EnumerateItems()
                    .Select(x => _nodeItemsControl.FindAssociatedNodeItem(x))
                    .ToArray();

                var clickedNodes = nodes
                    .Where(x => x.IsMouseOver)
                    .ToArray();

                var connections = _connectionItemsControl
                    .FindVisualChildrenWithType<ConnectionShape>()
                    .ToArray();

                var clickedConnections = connections
                    .Where(x => x.HitTestCircle(args.GetPosition(_canvas),12))
                    .ToArray();

                var clickedPlugs = get_mouse_over_plugs(clickedNodes, out var sourcePlugType);

                IControlSelector selector = MakeControlSelector();
                selector.OnSelect(new SelectInfo(
                    nodes.Select(x=>x.DataContext).OfType<ISelectable>().ToArray(), 
                    clickedNodes.Select(x => x.DataContext).OfType<ISelectable>().ToArray(),
                    connections.Select(x => x.DataContext).OfType<ISelectable>().ToArray(),
                    clickedConnections.Select(x => x.DataContext).OfType<ISelectable>().ToArray()));

                if (clickedPlugs.Any())
                {
                    return new ConnectionCreateController(args,_canvas,nodes, clickedPlugs, _creatingConnectionItemsControl, CompletedCreateConnectionCommand,StartCreateConnectionCommand, sourcePlugType);
                }

                var selectedNodes = nodes
                    .Where(x => x.IsSelected)
                    .ToArray();
                if (selectedNodes.Any())
                {
                    return new NodeDragController(args, nodes, selectedNodes, _canvas, (int)GridSize, UseGridSnap, CompetedMoveNodeCommand);
                }
            }

            return null;
        }

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
