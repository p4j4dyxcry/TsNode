using System;
using System.Collections.Specialized;
using System.ComponentModel;
using TsCore.Foundation.Reactive;

namespace TsCore.Mvvm
{
    /// <summary>
    /// INotifyPropertyChangedのPropertyChangedでOnNextを実行するObserver
    /// Observable.FromEventを利用すると暗黙的にスケジューラ上で動作させることになりパフォーマンスが悪いので
    /// 代わりにこのクラスを使うことでRx内部で保持されているスケジューラから再スケジュールされないようになり高速に動作します
    /// </summary>
    public class PropertyObserver<T> : ObserverProduct<PropertyChangedEventArgs,PropertyChangedEventArgs>
        where T : INotifyPropertyChanged
    {
        private readonly T _propertyOwner;

        public PropertyObserver(T propertyOwner) : base(x=>x)
        {
            _propertyOwner = propertyOwner;
            _propertyOwner.PropertyChanged += OnPropertyChanged;
        }
        
        private void OnPropertyChanged(object s, PropertyChangedEventArgs e)
        {
            OnNext(e);
        }
        
        protected override void Dispose(bool disposed)
        {
            if (disposed is false)
            {
                _propertyOwner.PropertyChanged -= OnPropertyChanged;
            }
        }
    }

    /// <summary>
    /// INotifyCollectionChangedのCollectionChangedでOnNextを実行するObserver
    /// Observable.FromEventを利用すると暗黙的にスケジューラ上で動作させることになりパフォーマンスが悪いので
    /// 代わりにこのクラスを使うことでRx内部で保持されているスケジューラから再スケジュールされないようになり高速に動作します
    /// </summary>
    public class CollectionObserver : ObserverProduct<NotifyCollectionChangedEventArgs,NotifyCollectionChangedEventArgs>
    {
        private readonly INotifyCollectionChanged _collectionOwner;

        public CollectionObserver(INotifyCollectionChanged property)
        {
            _collectionOwner = property;
            _collectionOwner.CollectionChanged += OnCollectionChanged;
        }
        
        private void OnCollectionChanged(object s, NotifyCollectionChangedEventArgs e)
        {
            OnNext(e);
        }

        protected override void Dispose(bool disposed)
        {
            if (disposed is false)
            {
                _collectionOwner.CollectionChanged -= OnCollectionChanged;                
            }
        }
    }

    public static class PropertyObserverExtensions
    {
        public static IObservable<PropertyChangedEventArgs> AsObservable(this INotifyPropertyChanged sender)
        {
            return new PropertyObserver<INotifyPropertyChanged>(sender);
        }
        public static IObservable<NotifyCollectionChangedEventArgs> AsObservable(this INotifyCollectionChanged sender)
        {
            return new CollectionObserver(sender);
        }
        public static IObservable<PropertyChangedEventArgs> ObserveProperty(this INotifyPropertyChanged sender, string propertyName)
        {
            return sender.AsObservable()
                .Where(x => x.PropertyName == propertyName);
        }
    }
}