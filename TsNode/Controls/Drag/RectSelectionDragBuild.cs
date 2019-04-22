using System.Windows;
using System.Windows.Input;
using TsNode.Interface;

namespace TsNode.Controls.Drag
{
    public class RectSelectionDragBuild : IDragControllerBuild
    {
        public int Priority { get; }

        private readonly DragControllerBuilder _dragControllerBuilder;

        private Style SelectionRectangleStyle { get; }

        public RectSelectionDragBuild(DragControllerBuilder dragControllerBuilder, int priority, Style selectionRectangleStyle)
        {
            _dragControllerBuilder = dragControllerBuilder;
            Priority = priority;
            SelectionRectangleStyle = selectionRectangleStyle;
        }

        public bool TryBuild()
        {
            return _dragControllerBuilder.MouseEventArgs.LeftButton == MouseButtonState.Pressed;
        }

        public IDragController Build()
        {
            var setupArgs = new SelectionRectDragControllerSetupArgs(
                _dragControllerBuilder.InputElement, 
                _dragControllerBuilder.MouseEventArgs,
                _dragControllerBuilder.Nodes, 
                _dragControllerBuilder.ConnectionShapes)
            {
                SelectionChangedCommand = _dragControllerBuilder.SelectionChangedCommand,
                RectangleStyle = SelectionRectangleStyle
            };
            return new SelectionRectDragController(setupArgs);
        }
    }
}
