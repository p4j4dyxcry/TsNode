using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TsCore.Foundation
{
    public static class EnumerableExtensions
    {
        public static CompositeDisposable ToCompositeDisposable(this IEnumerable<IDisposable> source)
        {
            return new CompositeDisposable(source);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
        }

        public static ReadOnlyObservableCollection<T> ToObservableCollection<T>(this ObservableCollection<T> source)
        {
            return new ReadOnlyObservableCollection<T>(source);
        }
    }
}