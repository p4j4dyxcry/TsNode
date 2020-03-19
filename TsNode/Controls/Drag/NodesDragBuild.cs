using System.Linq;
using System.Windows.Input;
using TsNode.Controls.Drag.Controller;
using TsNode.Interface;

namespace TsNode.Controls.Drag
{
    public class NodesDragBuild : IDragControllerBuild
    {
        public int Priority { get; }
        
        public InfiniteScrollViewer ScrollViewer { get; set; }

        private readonly DragControllerBuilder _dragControllerBuilder;

        private int SnapGridSize { get; } 
        private bool UseSnapGrid { get; }

        public NodesDragBuild(DragControllerBuilder dragControllerBuilder, int priority , bool useSnapGrid , int snapGridSize)
        {
            _dragControllerBuilder = dragControllerBuilder;
            Priority = priority;
            UseSnapGrid = useSnapGrid;
            SnapGridSize = snapGridSize;
        }

        public bool TryBuild()
        {
            return _dragControllerBuilder.MouseButton == MouseButton.Left &&
                   _dragControllerBuilder.SelectedNodes.Any();
        }

        public IDragController Build()
        {
            var setupArgs = new NodeDragControllerSetupArgs(
                _dragControllerBuilder.InputElement,
                _dragControllerBuilder.SelectedNodes,
                _dragControllerBuilder.CompletedNodeDragCommand,
                ScrollViewer)
            {
                GridSize = SnapGridSize,
                UseSnapGrid = UseSnapGrid,
            };

            return new NodesDragController(setupArgs);
        }
    }
}
