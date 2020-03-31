using System.Windows.Input;
using TsNode.Extensions;
using TsNode.Foundations;
using TsNode.Interface;

namespace TsNode.Controls.Drag.Controller
{
    internal class ViewportDragController : IDragController
    {
        public void Cancel()
        {

        }

        public bool CanDragStart(DragControllerEventArgs args)
        {
            return args.Button == MouseButton.Middle;
        }

        public void OnStartDrag(DragControllerEventArgs args)
        {

        }

        public void OnDragEnd(DragControllerEventArgs args)
        {

        }

        public void OnDragMoving(DragControllerEventArgs args)
        {
            if (_scrollViewer != null)
            {
                _scrollViewer?.Translate(args.Delta.X, args.Delta.Y);                
                _scrollViewer.UpdateScrollBar();
            }
        }
        private NetworkView NetworkView { get; }

        private readonly InfiniteScrollViewer _scrollViewer;
        
        public ViewportDragController(NetworkView networkView )
        {
            NetworkView = networkView;
            _scrollViewer = NetworkView.FindChild<InfiniteScrollViewer>(x=>true);
        }
    }
}
