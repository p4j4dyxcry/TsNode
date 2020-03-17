using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TsNode.Extensions
{
    internal static class ItemsControlExtensions
    {
        public static IEnumerable<T> GetAsContentControl<T>(this ItemsControl self) where T : FrameworkElement
        {
            return Enumerable.Range(0, self.Items.Count)
                .Select(x => self.Items.GetItemAt(x))
                .OfType<T>();
        }

        public static IEnumerable<ListBoxItem> GetListBoxItems(this ListBox self)
        {
            return GetAsContentControl<ListBoxItem>(self);
        }

        public static IEnumerable<object> EnumerateItems(this ItemsControl self)
        {
            return Enumerable.Range(0, self.Items.Count)
                .Select(x => self.Items.GetItemAt(x));
        }
    }
}