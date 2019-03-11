using System.Windows;
using System.Windows.Controls;

namespace TsNode.Controls.Plug
{
    public class PlugItemsControl : ItemsControl
    {
        public PlugControl FindAssociatedPlugItem(object dataContext)
            => ItemContainerGenerator.ContainerFromItem(dataContext) as PlugControl;

        protected override DependencyObject GetContainerForItemOverride()
            => new PlugControl();

        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is PlugControl;
    }
}
