using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TsNode.Controls.Connection;
using TsNode.Controls.Node;
using TsNode.Interface;

namespace TsNode.Controls.Drag
{
    public interface IDragControllerBuild
    {
        int　Priority { get; }

        bool TryBuild();

        IDragController Build();
    }

    /// <summary>
    /// ドラッグコントローラを作成するビルダークラス
    /// </summary>
    public class DragControllerBuilder 
    {
        public IInputElement InputElement { get; }
        public MouseButton MouseButton { get; }
        public ModifierKeys ModifierKeys { get; }
        public INodeControl[] Nodes { get; }
        public ConnectionShape[] ConnectionShapes { get; }
        public INodeControl[] SelectedNodes { get; }
        public ICommand StartConnectionCommand { get; private set; }
        public ICommand CancelConnectionCommand { get; private set; }
        public ICommand ConnectConnectionCommand { get; private set; }
        public ICommand SelectionChangedCommand { get; private set; }
        public ICommand CompletedNodeDragCommand { get; private set; }

        private　IList<IDragControllerBuild> BuildTargets { get; } = new List<IDragControllerBuild>();

        public DragControllerBuilder(
            IInputElement inputElement, 
            MouseButton button,
            ModifierKeys modifierKeys,
            INodeControl[] nodes, 
            ConnectionShape[] connectionShapes)
        {
            InputElement = inputElement;
            Nodes = nodes;
            ConnectionShapes = connectionShapes;
            MouseButton = button;
            ModifierKeys = modifierKeys;

            SelectedNodes = Nodes.Where(x => x.IsSelected).ToArray();
        }

        public DragControllerBuilder AddBuildTarget(IDragControllerBuild controller)
        {
            BuildTargets.Add(controller);
            return this;
        }

        public DragControllerBuilder SetConnectionCommand(ICommand startConnectionCommand , ICommand completedConnectionCommand , ICommand cancelConnectionCommand)
        {
            StartConnectionCommand = startConnectionCommand;
            ConnectConnectionCommand = completedConnectionCommand;
            CancelConnectionCommand = cancelConnectionCommand;
            return this;
        }

        public DragControllerBuilder SetNodeDragCompletedCommand(ICommand command)
        {
            CompletedNodeDragCommand = command;
            return this;
        }

        public DragControllerBuilder SetSelectionChangedCommand(ICommand selectionChangedCommand)
        {
            SelectionChangedCommand = selectionChangedCommand;
            return this;
        }

        public bool ModifierShift()
        {
            return (ModifierKeys & ModifierKeys.Shift) > 0;
        }

        public bool ModifierControl()
        {
            return (ModifierKeys & ModifierKeys.Control) > 0;
        }

        public bool ModifierAlt()
        {
            return (ModifierKeys & ModifierKeys.Alt) > 0;
        }

        public bool ModifierNone()
        {
            return ModifierKeys is 0;
        }
        
        
        /// <summary>
        /// ドラッグコントローラを作成します
        /// 登録されたドラッグコントローラのPriority順に生成チェックを行い最初に生成に成功したコントローラを返します。
        /// </summary>
        /// <returns></returns>
        public IDragController Build()
        {
            foreach (var buildTarget in BuildTargets.OrderBy(x=>x.Priority))
            {
                if (buildTarget.TryBuild())
                    return buildTarget.Build();
            }
            return null;
        }
    }
}
