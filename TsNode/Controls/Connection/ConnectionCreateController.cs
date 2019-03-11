using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TsNode.Controls.Node;
using TsNode.Controls.Plug;
using TsNode.Interface;

namespace TsNode.Controls.Connection
{
    public class ConnectionCreateController : IDragController
    {
        private readonly IConnectionViewModel[] _connections;
        private readonly IPlugViewModel[] _dragSourcePlugs;
        private readonly Dictionary<IPlugViewModel, IConnectionViewModel> _plugToConnectionViewModels;
        private readonly ConnectionItemsControl _connectionItemsControl;
        private readonly NodeControl[] _nodes;
        private readonly ICommand _connectionCreated;
        private readonly SourcePlugType _sourcePlugType;
        private readonly IInputElement _inputElement;
        private bool IsCreated { get; set; }
        public bool CanDragStart(object sender, MouseEventArgs args)
        {
            return _connections.All(x=>x!=null);
        }

        public void OnDrag(object sender, MouseEventArgs args)
        {
            if (_connectionItemsControl.Items.IsEmpty && IsCreated is false)
            {
                foreach (var plug in _dragSourcePlugs)
                {
                    var connection = _plugToConnectionViewModels[plug];
                    if (_sourcePlugType == SourcePlugType.Input)
                        connection.DestPlug = plug;
                    else
                        connection.SourcePlug = plug;
                    _connectionItemsControl.Items.Add(connection);
                }
            }

            foreach (var connection in _connectionItemsControl.FindVisualChildrenWithType<ConnectionShape>())
            {
                var point = args.GetPosition(_inputElement);
                if (_sourcePlugType == SourcePlugType.Output)
                {
                    connection.DestX = point.X;
                    connection.DestY = point.Y;
                }
                if (_sourcePlugType == SourcePlugType.Input)
                {
                    connection.SourceX = point.X;
                    connection.SourceY = point.Y;
                }

            }

            if (args.LeftButton != MouseButtonState.Pressed)
            {
                if (IsCreated is false)
                    create_connection();
            }
        }

        public void DragEnd(object sender, MouseEventArgs args)
        {
            if(IsCreated is false)
                create_connection();
        }

        public void Cancel()
        {
            _connectionItemsControl.Items.Clear();
            IsCreated = true;
        }

        private void create_connection()
        {
            var targetPlugs = _nodes
                .SelectMany(x => _sourcePlugType == SourcePlugType.Input ? x.GetOutputPlugs() : x.GetInputPlugs() )
                .Where(x => x.IsMouseOver)
                .ToArray();

            var connectTarget = targetPlugs.FirstOrDefault()?.DataContext as IConnectTarget
                ?? _nodes.FirstOrDefault(x => x.IsMouseOver)?.DataContext as IConnectTarget;

            if (connectTarget != null)
            {
                if (_dragSourcePlugs.All(x => connectTarget.TryConnect(
                    new ConnectInfo()
                    {
                        Sender = x,
                        SenderType = _sourcePlugType,
                        Connection = _plugToConnectionViewModels[x]
                    })))
                {
                    foreach (var dragSourcePlug in _dragSourcePlugs)
                    {
                        _connectionCreated?.Execute(new CompletedCreateConnectionEventArgs(_plugToConnectionViewModels[dragSourcePlug]));
                    }
                }
            }

            Cancel();
        }

        public ConnectionCreateController(
            MouseEventArgs args , 
            IInputElement sender, 
            NodeControl[] allNodes , 
            PlugControl[] dragPlugs , 
            ConnectionItemsControl connectionItemsControl , 
            ICommand connectionCreated ,
            ICommand startConnectionCreated, 
            SourcePlugType sourcePlugType )
        {
            _nodes = allNodes;
            _inputElement = sender;

            _dragSourcePlugs = dragPlugs
                .Select(x => x.DataContext)
                .OfType<IPlugViewModel>()
                .ToArray();

            _plugToConnectionViewModels = _dragSourcePlugs
                .ToDictionary(x => x, x => x.StartConnection());

            _connections = _plugToConnectionViewModels
                .Select(x => x.Value)
                .ToArray();

            _connectionItemsControl = connectionItemsControl;
            _connectionCreated = connectionCreated;
            _sourcePlugType = sourcePlugType;
            startConnectionCreated?.Execute(new StartCreateConnectionEventArgs(_dragSourcePlugs));
        }
    }
}
