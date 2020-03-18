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
    }
    
    public interface IDragController
    {
        bool CanDragStart(DragControllerEventArgs args);
        void OnDrag(DragControllerEventArgs args);
        void DragEnd();
        void Cancel();
    }
}
