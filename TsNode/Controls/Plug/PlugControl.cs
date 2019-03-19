using System.Windows;
using System.Windows.Controls;
using TsNode.Controls.Node;

namespace TsNode.Controls.Plug
{
    /// <summary>
    /// プラグコントロール
    /// </summary>
    public class PlugControl : ContentControl
    {
        private NodeControl _parentNode;
        public NodeControl ParentNode => _parentNode ?? (_parentNode = this.FindVisualParentWithType<NodeControl>());

        // ノードから見たプラグ座標を取得する
        public Point GetNodeFromPoint(Point from)
        {
            return TranslatePoint(from, ParentNode);
        }
    }
}