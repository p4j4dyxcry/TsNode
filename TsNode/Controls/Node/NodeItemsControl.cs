using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TsNode.Extensions;
using TsNode.Interface;

namespace TsNode.Controls.Node
{
    public class NodeItemsControl : ItemsControl
    {
        public NodeControl FindAssociatedNodeItem(object dataContext)
            => ItemContainerGenerator.ContainerFromItem(dataContext) as NodeControl;

        protected override DependencyObject GetContainerForItemOverride()
            => new NodeControl();

        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is NodeControl;

        public NodeItemsControl()
        {
            ItemsPanel = new ItemsPanelTemplate()
            {
                VisualTree = new FrameworkElementFactory(typeof(Canvas))
            };
        }

        public INodeControl[] GetNodes(Func<NodeControl,bool> function = null)
        {
            return this.EnumerateItems()
                .Select(FindAssociatedNodeItem)
                .Where(x=>function?.Invoke(x) ?? true)
                .OfType<INodeControl>()
                .ToArray();
        }
    }
}
