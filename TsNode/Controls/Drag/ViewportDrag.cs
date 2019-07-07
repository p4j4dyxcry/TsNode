using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
            var offset = args.GetPosition(NetworkView) - MouseStart;

            NetworkView.Translate(offset.X, offset.Y);
            NetworkView.GridUpdate();

            MouseStart = args.GetPosition(NetworkView);
        }

        private Point MouseStart { get; set; }
        private NetworkView NetworkView { get; }
        private FrameworkElement _inputElement;
        public ViewportDrag(NetworkView networkView , MouseEventArgs mouseEventArgs , FrameworkElement target)
        {
            MouseStart = mouseEventArgs.GetPosition(target);
            NetworkView = networkView;
            _inputElement = target;
        }
    }
}
