using System;
using System.Windows;
using System.Windows.Controls;

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

        public event Action<object, System.EventArgs> SelectionChanged;

        public void RaiseSelectionChanged(object sender , System.EventArgs args)
        {
            SelectionChanged?.Invoke(sender,args);
        }

        public NodeItemsControl()
        {
            ItemsPanel = new ItemsPanelTemplate()
            {
                VisualTree = new FrameworkElementFactory(typeof(Canvas))
            };
        }
    }
}
