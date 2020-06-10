using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using TsNode.Preset.Foundation;

namespace TsNode.Preset
{
    internal enum BindMode
    {
        OneWay,
        TwoWay,
    }

    internal interface ISimpleValueConverter
    {
        object Convert(object sender);
        object ConvertBack(object sender);
    }

    internal class DefaultConverter : ISimpleValueConverter
    {
        public static ISimpleValueConverter Instance { get; } = new DefaultConverter();
        
        public object Convert(object sender)
        {
            return sender;
        }

        public object ConvertBack(object sender)
        {
            return sender;
        }
    }
    
    
    public static class Extensions
    {
        internal static void BindModel(this INotifyPropertyChanged sender, INotifyPropertyChanged model , BindMode bindMode = BindMode.OneWay , ISimpleValueConverter converter = null)
        {
            converter = converter ?? DefaultConverter.Instance;
            
            model.PropertyChanged += (s, e) =>
            {
                FastReflection.SetProperty(sender, e.PropertyName, converter.Convert(FastReflection.GetProperty(model, e.PropertyName)));
            };
            if (bindMode == BindMode.TwoWay)
            {
                sender.PropertyChanged += (s, e) =>
                {
                    FastReflection.SetProperty(model, e.PropertyName, converter.ConvertBack(FastReflection.GetProperty(sender, e.PropertyName)));
                };                
            }
        }
        
        public static ObservableCollection<TDest> Mapping<TSource, TDest>(this ObservableCollection<TSource> from, Func<TSource, TDest> selector)
        {
            var collection = new ObservableCollection<TDest>(from.Select(selector));
            var pairs = new List<Tuple<TSource, TDest>>();

            for (int i = 0; i < from.Count; ++i)
            {
                pairs.Add(new Tuple<TSource, TDest>(from[i],collection[i]));
            }

            from.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    var startIndex = 0;

                    var pair = pairs.FirstOrDefault();
                    if (pair != null)
                    {
                        startIndex = collection.IndexOf(pair.Item2);
                    }

                    var targetIndex = startIndex + e.NewStartingIndex;

                    var source = (TSource) e.NewItems[0];
                    var dest = selector(source);
                    pairs.Add(new Tuple<TSource, TDest>(source,dest));
                    collection.Insert(targetIndex,dest);
                }

                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    var source = (TSource) e.OldItems[0];
                    var item = pairs.First(x => ReferenceEquals(x.Item1 , source));
                    pairs.Remove(item);
                    collection.Remove(item.Item2);
                };
            };
            return collection;
        }
    }
}