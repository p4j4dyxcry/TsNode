using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using TsGui.Foundation;

namespace TsGui.Mvvm
{
    /// <summary>
    /// INotifyPropertyChangedのPropertyChangedでOnNextを実行するObserver
    /// Observable.FromEventを利用すると暗黙的にスケジューラ上で動作させることになりパフォーマンスが悪いので
    /// 代わりにこのクラスを使うことでRx内部で保持されているスケジューラから再スケジュールされないようになり高速に動作します
    /// </summary>
    public class PropertyObserver<T> : IObservable<PropertyChangedEventArgs>,
        IObserver<PropertyChangedEventArgs>, IDisposable where T : INotifyPropertyChanged
    {
        private readonly IList<IObserver<PropertyChangedEventArgs>> _observers =
            new List<IObserver<PropertyChangedEventArgs>>();

        private readonly T _property;

        public IDisposable Subscribe(IObserver<PropertyChangedEventArgs> observer)
        {
            _observers.Add(observer);
            return Disposable.Create(() => { _observers.Remove(observer); });
        }

        public void Dispose()
        {
            _property.PropertyChanged -= OnPropertyChanged;
        }

        private void OnPropertyChanged(object s, PropertyChangedEventArgs e)
        {
            //! OnNextでProperyChangedObserverがSubscribeされるとInvalidOperationExceptionが発生するのでArray化して置く
            var observersArray = _observers.ToArray();
            foreach (var o in observersArray)
                o.OnNext(e);
        }

        public PropertyObserver(T property)
        {
            _property = property;
            _property.PropertyChanged += OnPropertyChanged;
        }

        public void OnNext(PropertyChangedEventArgs value)
        {
            // このタイミングで後続に値を流す必要は無いので空実装です
            // 実際に値が発行されるのはPropertyChangedが呼ばれたときになるので
            // そのタイミングで後続に値を流します( observerのOnNextを呼び出す)
        }

        public void OnError(Exception error)
        {
            foreach (var o in _observers)
                o.OnError(error);
        }

        public void OnCompleted()
        {
            foreach (var o in _observers)
                o.OnCompleted();
        }
    }

    public sealed class PropertyObserver : PropertyObserver<INotifyPropertyChanged>
    {
        public PropertyObserver(INotifyPropertyChanged property) : base(property)
        {
        }
    }

    /// <summary>
    /// INotifyCollectionChangedのCollectionChangedでOnNextを実行するObserver
    /// Observable.FromEventを利用すると暗黙的にスケジューラ上で動作させることになりパフォーマンスが悪いので
    /// 代わりにこのクラスを使うことでRx内部で保持されているスケジューラから再スケジュールされないようになり高速に動作します
    /// </summary>
    public class CollectionObserver : IObservable<NotifyCollectionChangedEventArgs>,
        IObserver<NotifyCollectionChangedEventArgs>, IDisposable
    {
        private readonly IList<IObserver<NotifyCollectionChangedEventArgs>> _observers =
            new List<IObserver<NotifyCollectionChangedEventArgs>>();

        private readonly INotifyCollectionChanged _property;

        private void OnCollectionChanged(object s, NotifyCollectionChangedEventArgs e)
        {
            foreach (var o in _observers)
                o.OnNext(e);
        }

        public CollectionObserver(INotifyCollectionChanged property)
        {
            _property = property;
            _property.CollectionChanged += OnCollectionChanged;
        }

        public IDisposable Subscribe(IObserver<NotifyCollectionChangedEventArgs> observer)
        {
            _observers.Add(observer);
            return Disposable.Create(() => { _observers.Remove(observer); });
        }

        public void OnNext(NotifyCollectionChangedEventArgs value)
        {
            // このタイミングで後続に値を流す必要は無いので空実装です
            // 実際に値が発行されるのはCollectionChangedが呼ばれたときになるので
            // そのタイミングで後続に値を流します( observerのOnNextを呼び出す)
        }

        public void OnError(Exception error)
        {
            foreach (var o in _observers)
                o.OnError(error);
        }

        public void OnCompleted()
        {
            foreach (var o in _observers)
                o.OnCompleted();
        }

        public void Dispose()
        {
            _property.CollectionChanged -= OnCollectionChanged;
        }
    }
}