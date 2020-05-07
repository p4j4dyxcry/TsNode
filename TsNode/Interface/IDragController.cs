using System.Windows;
using System.Windows.Input;

namespace TsNode.Interface
{
    public class DragControllerEventArgs
    {
        public Point StartPoint { get; }
        public Point  CurrentPoint { get; }
        public Vector Vector { get; }
        
        public Vector Delta { get; }
        
        public MouseButton Button { get; }
        
        public DragControllerEventArgs(Point startPoint , Point currentPoint, Vector delta , MouseButton button)
        {
            StartPoint = startPoint;
            CurrentPoint = currentPoint;
            Vector = CurrentPoint - StartPoint;
            Delta = delta;
            Button = button;
        }

        public DragControllerEventArgs CreateUpdatedArgs(Point current)
        {
            return new DragControllerEventArgs(StartPoint , current , current - CurrentPoint , Button);
        }
        
        public DragControllerEventArgs CreateEndArgs()
        {
            return this;
        }
    }

    public interface IUseMouseCaptureTarget
    {
        void Capture();
        
        void ReleaseCapture();
    }
    
    public interface IDragController
    {
        bool CanDragStart(DragControllerEventArgs args);
        void OnStartDrag(DragControllerEventArgs args);
        void OnDragMoving(DragControllerEventArgs args);
        void OnDragEnd(DragControllerEventArgs args);
        void Cancel();
    }
}
