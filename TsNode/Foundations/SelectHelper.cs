using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TsNode.Interface;

namespace TsNode.Foundations
{
    internal static class SelectHelper
    {
        public static ISelectable[] AddSelect(IEnumerable<ISelectable> allItems, IEnumerable<ISelectable> targetItems)
        {
            var result = new List<ISelectable>();
            foreach (var i in targetItems)
            {
                if (i.IsSelected is false)
                    result.Add(i);
                i.IsSelected = true;
            }
            return result.ToArray();
        }

        public static ISelectable[] ToggleSelect(IEnumerable<ISelectable> allItems, IEnumerable<ISelectable> targetItems)
        {
            var itemsArray = targetItems as ISelectable[] ?? targetItems.ToArray();
            foreach (var i in itemsArray)
                i.IsSelected = !i.IsSelected;
            return itemsArray;
        }

        public static ISelectable[] SingleSelect(IEnumerable<ISelectable> allItems, IEnumerable<ISelectable> targetItems)
        {
            var result = new List<ISelectable>();

            var targetItemsArray = targetItems.ToArray();

            if (targetItemsArray.Any(x=>x.IsSelected) is false)
            {
                foreach (var i in allItems)
                {
                    if (i.IsSelected)
                        result.Add(i);
                    i.IsSelected = false;
                }
            }

            foreach (var i in targetItemsArray)
            {
                if (i.IsSelected is false)
                    result.Add(i);
                i.IsSelected = true;
            }
            return result.ToArray();
        }

        public static ISelectable[] OnlySelect(IEnumerable<ISelectable> allItems, IEnumerable<ISelectable> targetItems)
        {
            var result = new List<ISelectable>();

            var targetItemsArray = targetItems.ToArray();

            foreach (var i in allItems)
            {
                bool flag = i.IsSelected;
                if (targetItemsArray.Contains(i))
                    i.IsSelected = true;
                else
                    i.IsSelected = false;

                if (flag != i.IsSelected)
                    result.Add(i);
            }
            return result.ToArray();
        }
    }
    
    internal static class SelectableExtensions
    {
        public static ISelectable[] ToSelectable<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.OfType<ISelectable>().ToArray();
        }

        public static ISelectable[] ToSelectableDataContext(this IEnumerable<FrameworkElement> enumerable)
        {
            return enumerable.Select(x=>x.DataContext).OfType<ISelectable>().ToArray();
        }
    }
}