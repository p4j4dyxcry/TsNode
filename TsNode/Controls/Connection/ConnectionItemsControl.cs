using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TsNode.Extensions;

namespace TsNode.Controls.Connection
{
    public class ConnectionItemsControl : ItemsControl
    {
        public ConnectionItemsControl()
        {
            ItemsPanel = new ItemsPanelTemplate()
            {
                VisualTree = new FrameworkElementFactory(typeof(Canvas))
            };
        }

        public ConnectionShape[] GetConnectionShapes()
        {
            return this.FindVisualChildrenWithType<ConnectionShape>().ToArray();
        }
    }
}
