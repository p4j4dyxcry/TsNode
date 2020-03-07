using System.Linq;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using TsControls.EventListeners;

namespace TsControls.Behaviors
{
    public sealed class ListBoxAutoScrollBehavior : Behavior<ListBox>
    {
        private CollectionChangedListener _itemChangedListener;

        protected override void OnAttached()
        {
            base.OnAttached();
            _itemChangedListener = new CollectionChangedListener(AssociatedObject.Items, (s, e) =>
            {
                var lastItem = AssociatedObject.Items.Cast<object>().LastOrDefault();
                if (lastItem == null)
                    return;
                AssociatedObject.ScrollIntoView(lastItem);
            });
        }

        protected override void OnDetaching()
        {
            _itemChangedListener.Dispose();
            base.OnDetaching();
        }
    }
}