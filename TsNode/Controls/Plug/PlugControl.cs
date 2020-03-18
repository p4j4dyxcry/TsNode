using System.Windows;
using System.Windows.Controls;
using TsNode.Controls.Node;
using TsNode.Extensions;
using TsNode.Interface;

namespace TsNode.Controls.Plug
{
    /// <summary>
    /// プラグコントロール
    /// </summary>
    public class PlugControl : ContentControl , IPlugControl
    {
        private NodeControl _parentNode;
        public INodeControl ParentNode => _parentNode ?? (_parentNode = this.FindVisualParentWithType<NodeControl>());

        // ノードから見たプラグ座標を取得する
        public Point GetNodeFromPoint(Point from)
        {
            return TranslatePoint(from, ParentNode as NodeControl);
        }
    }
}