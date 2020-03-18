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
        public INodeControl[] Nodes { get; }
        public ConnectionShape[] ConnectionShapes { get; }
        public INodeControl[] SelectedNodes { get; }
        public ICommand StartConnectionCommand { get; private set; }
        public ICommand ConnectConnectionCommand { get; private set; }
        public ICommand SelectionChangedCommand { get; private set; }
        public ICommand CompletedNodeDragCommand { get; private set; }

        private　IList<IDragControllerBuild> BuildTargets { get; } = new List<IDragControllerBuild>();

        public DragControllerBuilder(
            IInputElement inputElement, 
            MouseButton button,
            INodeControl[] nodes, 
            ConnectionShape[] connectionShapes)
        {
            InputElement = inputElement;
            Nodes = nodes;
            ConnectionShapes = connectionShapes;
            MouseButton = button;

            SelectedNodes = Nodes.Where(x => x.IsSelected).ToArray();
        }

        public DragControllerBuilder AddBuildTarget(IDragControllerBuild controller)
        {
            BuildTargets.Add(controller);
            return this;
        }

        public DragControllerBuilder SetConnectionCommand(ICommand startConnectionCommand , ICommand completedConnectionCommand)
        {
            StartConnectionCommand = startConnectionCommand;
            ConnectConnectionCommand = completedConnectionCommand;
            return this;
        }

        public DragControllerBuilder SetNodeDragControllerBuilder(ICommand command)
        {
            CompletedNodeDragCommand = command;
            return this;
        }

        public DragControllerBuilder SetSelectionChangedCommand(ICommand selectionChangedCommand)
        {
            SelectionChangedCommand = selectionChangedCommand;
            return this;
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
