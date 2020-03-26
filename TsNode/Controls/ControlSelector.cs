using System.Linq;
using System.Windows.Input;
using TsNode.Foundations;
using TsNode.Interface;

namespace TsNode.Controls
{
    /// <summary>
    /// シンプルなキーボード入力によって選択状態を切り替えるセレクタ
    /// </summary>
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
            SelectionType selectionType = SelectionType.None;

            //! 選択を反転
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                selectionChanged = SelectHelper.ToggleSelect(selectInfo.AllNodes.Concat(selectInfo.Connections), selectInfo.NewSelectNodes.Concat(selectInfo.NewConnections));
                selectionType = SelectionType.Toggle;
            }
            //! 追加選択
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                selectionChanged = SelectHelper.AddSelect(selectInfo.AllNodes.Concat(selectInfo.Connections), selectInfo.NewSelectNodes.Concat(selectInfo.NewConnections));
                if(selectionChanged.Any())
                    selectionType = SelectionType.Single;
            }

            //! 選択されたものだけを選択するが、既に選択済みの場合は何もしない(Nodeをまとめてドラッグする際にこのモードを利用する)
            else if (selectInfo.NewSelectNodes.Any())
            {
                selectionChanged = SelectHelper.SingleSelect(selectInfo.AllNodes.Concat(selectInfo.Connections), selectInfo.NewSelectNodes);
                selectionType = SelectionType.Single;
            }

            //! 選択されたものだけを選択状態とする
            else
            {
                selectionChanged = SelectHelper.OnlySelect(selectInfo.AllNodes.Concat(selectInfo.Connections), selectInfo.NewConnections);
                selectionType = SelectionType.Single;
            }
            
            //! 選択が変更されたことをイベントで通知する(undo / redo　等に使用)
            if(selectionChanged.Any())
                SelectionChanged?.Execute(new SelectionChangedEventArgs(selectionChanged,selectionType));
        }
    }
}
