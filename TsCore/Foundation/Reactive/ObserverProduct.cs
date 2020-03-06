using System;
using System.Collections.Generic;
using System.Linq;

namespace TsGui.Foundation.Reactive
{
    public class ObserverProduct<T,T2>  : IObserver<T>,IObservable<T2>, IDisposable
    {
        private readonly IList<IObserver<T2>> _observers =
            new List<IObserver<T2>>();

        private readonly Func<T, T2> _converter;

        protected ObserverProduct(Func<T, T2> converter = null)
        {
            _converter = converter ?? (x => (T2)(object)x);
        }
        
        public IDisposable Subscribe(IObserver<T2> observer)
        {
            _observers.Add(observer);
            return SubscribeRaw(Disposable.Create(() =>
            {
                _observers.Remove(observer);
                if(_observers.Count is 0)
                    this.Dispose();
            }));
        }

        protected virtual IDisposable SubscribeRaw(IDisposable disposable)
        {
            return disposable;
        }

        private bool _isDisposed = false;
        
        public void Dispose()
        {
            Dispose(_isDisposed);
            _isDisposed = true;
        }

        protected virtual void Dispose(bool disposed)
        {
            // not Impl
        }

        public virtual void OnNext(T value)
        {
            foreach (var o in _observers.ToArray())
                o.OnNext(_converter(value));
        }

        public void OnError(Exception error)
        {
            foreach (var o in _observers.ToArray())
                o.OnError(error);
        }

        public void OnCompleted()
        {
            foreach (var o in _observers.ToArray())
                o.OnCompleted();
        }
    }
    
    public class ObserveSelectorBase<T,T2> : ObserverProduct<T,T2>
    {
        private readonly IDisposable _disposable;
        
        protected ObserveSelectorBase(IObservable<T> observable, Func<T, T2> converter) : base(converter)
        {
            _disposable = observable.Subscribe(this);
        }
        
        protected override void Dispose(bool disposed)
        {
            if(disposed is false)
                _disposable.Dispose();
        } 
    }
    
    public class ObserverBase<T> : ObserveSelectorBase<T,T>
    {
        private readonly IDisposable _disposable;
        
        protected ObserverBase(IObservable<T> observable) : base(observable,DefaultConverter)
        {
            _disposable = observable.Subscribe(this);
        }
        
        protected override void Dispose(bool disposed)
        {
            if(disposed is false)
                _disposable.Dispose();
        } 
        
        private static T DefaultConverter(T t)
        {
            return t;
        }
    }
}