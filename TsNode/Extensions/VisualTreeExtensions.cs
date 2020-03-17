using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TsNode.Extensions
{
    internal static class VisualTreeExtensions
    {
        public static TParent FindVisualParentWithType<TParent>(this DependencyObject childElement)
            where TParent : class
        {
            var parentElement = (FrameworkElement)VisualTreeHelper.GetParent(childElement);
            if (parentElement != null)
            {
                if (parentElement is TParent parent)
                {
                    return parent;
                }

                return FindVisualParentWithType<TParent>(parentElement);
            }

            return null;
        }

        public static IEnumerable<TChild> FindVisualChildrenWithType<TChild>(this DependencyObject root) 
            where TChild : FrameworkElement
        {
            var children = Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(root)).Select(x => VisualTreeHelper.GetChild(root, x)).ToArray();

            foreach (var child in children)
            {
                if (child is TChild t)
                    yield return t;

                foreach (var _ in child.FindVisualChildrenWithType<TChild>())
                    yield return _;
            }
        }

        public static T FindTemplate<T>(this Control root , string name) 
            where T : Visual
        {
            var template = root.Template.FindName(name, root);
            Debug.Assert(template != null, $"{name}が実装されていません。");

            var result = template as T;
            Debug.Assert(result != null, $"{name}は{typeof(T).Name}で実装する必要があります。");

            return result;
        }

        public static T FindChildWithName<T>(this FrameworkElement root, string name) 
            where T : FrameworkElement
        {
            return root.FindChild<T>(x => x.Name == name);
        }

        public static T FindChildWithDataContext<T>(this FrameworkElement root, object datContext) 
            where T : FrameworkElement
        {
            return root.FindChild<T>(x=>x.DataContext == datContext);
        }

        public static T FindChild<T>(this FrameworkElement root, Func<FrameworkElement, bool> compare) 
            where T : FrameworkElement
        {
            var children = Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(root)).Select(x => VisualTreeHelper.GetChild(root, x)).OfType<FrameworkElement>().ToArray();

            foreach (var child in children)
            {
                if (child is T t && compare(child))
                    return t;

                t = child.FindChild<T>(compare);
                if (t != null)
                    return t;
            }
            return null;
        }

        public static bool ContainChildren(this FrameworkElement root, FrameworkElement @object)
        {
            if (@object is null)
                return false;

            if (root == @object)
                return true;

            var children = Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(root)).Select(x => VisualTreeHelper.GetChild(root, x)).OfType<FrameworkElement>().ToArray();

            foreach (var child in children)
            {
                if (child == @object)
                    return true;

                if (child.ContainChildren(@object))
                    return true;
            }
            return false;
        }

        public static bool HitTestCircle(this Visual root, Point center , double radius)
        {
            var result = false;
            VisualTreeHelper.HitTest(root, null, 
                _ => 
                {
                    result = true; return HitTestResultBehavior.Stop;
                }, 
                new GeometryHitTestParameters(new EllipseGeometry(center,radius,radius)));
            return result;
        }

        public static bool HitTestRect(this Visual root, Point center, double width , double height)
        {
            var result = false;
            var rect = new Rect(center.X - width / 2, center.Y - height / 2, width, height);
            VisualTreeHelper.HitTest(root, null, 
                _ => 
                {
                    result = true; return HitTestResultBehavior.Stop;
                }, 
                new GeometryHitTestParameters(new RectangleGeometry(rect)));
            return result;
        }

        public static bool HitTestRect(this Visual root, Rect rect)
        {
            var result = false;
            VisualTreeHelper.HitTest(root, null,
                _ =>
                {
                    result = true; return HitTestResultBehavior.Stop;
                },
                new GeometryHitTestParameters(new RectangleGeometry(rect)));
            return result;
        }

        public static IEnumerable<T> GetHitTestChildrenWithRect<T>(this Visual root, Rect rect) where T : Visual
        {
            var result = new List<DependencyObject>();
            VisualTreeHelper.HitTest(root, null,
                x =>
                {
                    result.Add(x.VisualHit);
                    return HitTestResultBehavior.Continue;
                },
                new GeometryHitTestParameters(new RectangleGeometry(rect)));

            return result.Select(x => x.FindVisualParentWithType<T>())
                .Where(x => x != null)
                .Distinct();
        }
    }

}