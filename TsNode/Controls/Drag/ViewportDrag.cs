using System.Linq;
using System.Windows;
using System.Windows.Input;
using TsNode.Extensions;
using TsNode.Foundations;
using TsNode.Interface;
using DragEventArgs = System.Windows.DragEventArgs;

namespace TsNode.Controls.Drag
{
    public class ViewportDrag : IDragController
    {
        public void Cancel()
        {

        }

        public bool CanDragStart(DragControllerEventArgs args)
        {
            return args.Button == MouseButton.Middle;
        }

        public void DragEnd()
        {

        }

        public void OnDrag(DragControllerEventArgs args)
        {
            if (_scrollViewer != null)
            {
                _scrollViewer?.Translate(args.Delta.X, args.Delta.Y);                
                _scrollViewer.UpdateScrollBar();
            }
        }
        private NetworkView NetworkView { get; }

        private readonly InfiniteScrollViewer _scrollViewer;
        
        public ViewportDrag(NetworkView networkView )
        {
            NetworkView = networkView;
            _scrollViewer = NetworkView.FindChild<InfiniteScrollViewer>(x=>true);
        }
    }
}
