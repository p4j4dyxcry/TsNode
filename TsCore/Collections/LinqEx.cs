using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TsCore.Collections
{
    public static class LinqEx
    {
        public static TSource FindMin<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector)
        {
            return self.OrderBy(selector).FirstOrDefault();
        }

        public static TSource FindMax<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector)
        {
            return self.OrderBy(selector).LastOrDefault();
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> c)
        {
            return new ObservableCollection<T>(c);
        }

        public static ReadOnlyObservableCollection<T> ToReadOnlyObservableCollection<T>(this IEnumerable<T> c)
        {
            return new ReadOnlyObservableCollection<T>(c as ObservableCollection<T> ?? new ObservableCollection<T>(c));
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        {
            return new HashSet<T>(collection);
        }

        public static IEnumerable<T> Merge<T>(IEnumerable<T> self, params IEnumerable<T>[] collections)
        {
            foreach (var variable in self)
                yield return variable;

            foreach (var collection in collections)
            foreach (var variable in collection)
                yield return variable;
        }

        public static IList<T> ToList<T>(this IEnumerable self)
        {
            var enumerable = self as object[] ?? self.Cast<object>().ToArray();
            return new List<T>(enumerable.OfType<T>());
        }

        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> self)
        {
            return self.ToList();
        }

        public static InfiniteList<T> ToInfiniteList<T>(this IEnumerable<T> self)
        {
            return new InfiniteList<T>(self);
        }

        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable self)
        {
            var enumerable = self as object[] ?? self.Cast<object>().ToArray();
            return new List<T>(enumerable.OfType<T>());
        }

        public static T[] ToArray<T>(this IEnumerable self)
        {
            var enumerable = self as object[] ?? self.Cast<object>().ToArray();
            return enumerable.OfType<T>().ToArray();
        }
    }
}