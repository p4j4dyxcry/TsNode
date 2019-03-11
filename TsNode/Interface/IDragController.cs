using System.Windows.Input;

namespace TsNode.Interface
{
    public interface IDragController
    {
        bool CanDragStart(object sender, MouseEventArgs args);
        void OnDrag(object sender, MouseEventArgs args);
        void DragEnd(object sender, MouseEventArgs args);
        void Cancel();
    }
}
