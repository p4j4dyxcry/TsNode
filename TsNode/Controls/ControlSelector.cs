using System.Linq;
using System.Windows.Input;
using TsNode.Interface;

namespace TsNode.Controls
{
    public class ControlSelector : IControlSelector
    {
        private ICommand SelectionChanged { get; }
        public ControlSelector( ICommand command )
        {
            SelectionChanged = command;
        }

        public void OnSelect(SelectInfo selectInfo)
        {
            ISelectable[] selectionChanged;

            if (Keyboard.Modifiers == ModifierKeys.Control)
                selectionChanged = SelectUtility.ToggleSelect(selectInfo.AllNodes.Concat(selectInfo.Connections), selectInfo.NewSelectNodes.Concat(selectInfo.NewConnections));
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
                selectionChanged = SelectUtility.AddSelect(selectInfo.AllNodes.Concat(selectInfo.Connections), selectInfo.NewSelectNodes.Concat(selectInfo.NewConnections));
            else if (selectInfo.NewSelectNodes.Any())
                selectionChanged = SelectUtility.SingleSelect(selectInfo.AllNodes.Concat(selectInfo.Connections), selectInfo.NewSelectNodes);
            else
                selectionChanged = SelectUtility.OnlySelect(selectInfo.AllNodes.Concat(selectInfo.Connections), selectInfo.NewConnections);

            if(selectionChanged.Any())
                SelectionChanged?.Execute(new SelectionChangedEventArgs(selectionChanged));
        }
    }
}
