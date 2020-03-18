using System.Linq;
using System.Windows.Input;
using TsNode.Controls.Connection;
using TsNode.Controls.Drag.Controller;
using TsNode.Controls.Node;
using TsNode.Controls.Plug;
using TsNode.Interface;

namespace TsNode.Controls.Drag
{
    internal class ConnectionDragBuild : IDragControllerBuild
    {
        public int Priority { get; }

        private readonly DragControllerBuilder _builder;
        private readonly IPlugControl[] _clickedPlugs;
        private readonly SourcePlugType _sourcePlugType;

        private ConnectionItemsControl CreatingConnectionItemsControl { get; }

        public ConnectionDragBuild(DragControllerBuilder builder, int priority , ConnectionItemsControl creatingConnectionItemsControl)
        {
            Priority = priority;
            _builder = builder;
            CreatingConnectionItemsControl = creatingConnectionItemsControl;

            // クリックしたノードの中でプラグがクリックされているものを集める
            _clickedPlugs = get_mouse_over_plugs(_builder.SelectedNodes, out _sourcePlugType);
        }

        private IPlugControl[] get_mouse_over_plugs(INodeControl[] nodes, out SourcePlugType sourcePlugType)
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

        public bool TryBuild()
        {
            return _builder.MouseButton == MouseButton.Left && _clickedPlugs.Any();
        }

        public IDragController Build()
        {
            var setupArgs = new ConnectionCreateControllerSetupArgs(
                _builder.Nodes,
                _clickedPlugs,
                CreatingConnectionItemsControl,
                _builder.ConnectConnectionCommand,
                _builder.StartConnectionCommand,
                _sourcePlugType);

            return new ConnectionCreateController(setupArgs);
        }
    }
}
