using System.Linq;
using TsNode.Interface;

namespace TsNode.Controls.Drag
{
    public class NodeDragBuild : IDragControllerBuild
    {
        public int Priority { get; }

        private readonly DragControllerBuilder _dragControllerBuilder;

        private int SnapGridSize { get; } 
        private bool UseSnapGrid { get; }

        public NodeDragBuild(DragControllerBuilder dragControllerBuilder, int priority , bool useSnapGrid , int snapGridSize)
        {
            _dragControllerBuilder = dragControllerBuilder;
            Priority = priority;
            UseSnapGrid = useSnapGrid;
            SnapGridSize = snapGridSize;
        }

        public bool TryBuild()
        {
            return _dragControllerBuilder.SelectedNodes.Any();
        }

        public IDragController Build()
        {
            var setupArgs = new NodeDragControllerSetupArgs(
                _dragControllerBuilder.InputElement,
                _dragControllerBuilder.MouseEventArgs,
                _dragControllerBuilder.SelectedNodes,
                _dragControllerBuilder.CompletedNodeDragCommand)
            {
                GridSize = SnapGridSize,
                UseSnapGrid = UseSnapGrid,
            };

            return new NodeDragController(setupArgs);
        }
    }
}
