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
        public static readonly DependencyProperty OffsetXProperty = DependencyProperty.Register(
            "OffsetX", typeof(double), typeof(PlugControl), new PropertyMetadata(6d));

        public double OffsetX
        {
            get => (double) GetValue(OffsetXProperty);
            set => SetValue(OffsetXProperty, value);
        }

        public static readonly DependencyProperty OffsetYProperty = DependencyProperty.Register(
            "OffsetY", typeof(double), typeof(PlugControl), new PropertyMetadata(6d));

        public double OffsetY
        {
            get => (double) GetValue(OffsetYProperty);
            set => SetValue(OffsetYProperty, value);
        }
        
        private NodeControl _parentNode;
        public INodeControl ParentNode => _parentNode ?? (_parentNode = this.FindVisualParentWithType<NodeControl>());

        // ノードから見たプラグ座標を取得する
        private Point get_node_from_point(Point from)
        {
            return TranslatePoint(from, ParentNode as NodeControl);
        }
        
        public Point GetNodeFromPoint()
        {
            return get_node_from_point(new Point(OffsetX, OffsetY));
        }
    }
}