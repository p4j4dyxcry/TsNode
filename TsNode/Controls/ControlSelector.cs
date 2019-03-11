using System.Windows.Input;
using TsNode.Interface;

namespace TsNode.Controls
{
    public class ControlSelector : IControlSelector
    {
        public void OnSelect(SelectInfo selectInfo)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
                SelectUtility.ToggleSelect(selectInfo.AllNodes, selectInfo.NewSelectNodes);
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
                SelectUtility.AddSelect(selectInfo.AllNodes, selectInfo.NewSelectNodes);
            else
                SelectUtility.SingleSelect(selectInfo.AllNodes, selectInfo.NewSelectNodes);
        }
    }
}
