using System.Windows;
using System.Windows.Input;
using TsNode.Interface;

namespace TsNode.Controls.Drag
{
    public class ViewportDrag : IDragController
    {
        public void Cancel()
        {

        }

        public bool CanDragStart(object sender, MouseEventArgs args)
        {
            return args.MiddleButton == MouseButtonState.Pressed;
        }

        public void DragEnd(object sender, MouseEventArgs args)
        {

        }

        public void OnDrag(object sender, MouseEventArgs args)
        {
            if (args.MiddleButton != MouseButtonState.Pressed)
                return;

            var offset = args.GetPosition(NetworkView) - PrevPosition;
            PrevPosition = args.GetPosition(NetworkView);
            NetworkView.Translate(offset.X, offset.Y);
        }

        private Point PrevPosition { get; set; }
        private NetworkView NetworkView { get; }
        public ViewportDrag(NetworkView networkView , MouseEventArgs mouseEventArgs)
        {
            PrevPosition = mouseEventArgs.GetPosition(networkView);
            NetworkView = networkView;
        }
    }
}
