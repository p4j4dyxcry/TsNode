using System;
using System.Collections.Generic;
using System.Linq;

namespace TsCore.Foundation.Reactive
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
        private IDisposable _disposable;
        private readonly IObservable<T> _observable;

        protected void InitializeSubscribe()
        {
            _disposable = _observable.Subscribe(this);
        }
        
        protected ObserveSelectorBase(IObservable<T> observable, Func<T, T2> converter) : base(converter)
        {
            _observable = observable;
        }
        
        protected override void Dispose(bool disposed)
        {
            if(disposed is false)
                _disposable.Dispose();
        } 
    }
    
    public class ObserverBase<T> : ObserverProduct<T,T>
    {
        private IDisposable _disposable;
        
        private readonly IObservable<T> _observable;

        protected void InitializeSubscribe()
        {
            _disposable = _observable.Subscribe(this);
        }
        
        protected ObserverBase(IObservable<T> observable) : base(DefaultConverter)
        {
            _observable = observable;
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
    
    public class MultiObserverBase<T> : ObserverProduct<T,T>
    {
        private IDisposable _disposable;
        private readonly IList<IObservable<T>> _observables = new List<IObservable<T>>();
        protected readonly IList<SubscribeHandler<T>> Handlers = new List<SubscribeHandler<T>>();

        protected void InitializeSubscribe()
        {
            foreach (var param in _observables)
                Handlers.Add(new SubscribeHandler<T>(param,this));
            _disposable = new CompositeDisposable(Handlers);
        }
        
        protected MultiObserverBase(IObservable<T> observable , params IObservable<T>[] subPrams) : base(DefaultConverter)
        {
            _observables.Add(observable);
            foreach (var obs in subPrams)
            {
                _observables.Add(obs);
            }
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
    
    public class MultiListObserverBase<T> : MultiObserverBase<T>, IObservable<IList<T>> , IObserver<IList<T>>
    {
        private readonly IList<IObserver<IList<T>>> _listObservers = new List<IObserver<IList<T>>>();
        
        protected MultiListObserverBase(IObservable<T> observable , params IObservable<T>[] subPrams):base(observable,subPrams)
        {
            
        }
        
        public IDisposable Subscribe(IObserver<IList<T>> observer)
        {
            _listObservers.Add(observer);
            return Disposable.Create(() =>
            {
                _listObservers.Remove(observer);
                if (_listObservers.Count is 0)
                    this.Dispose();
            });
        }

        public void OnNext(IList<T> value)
        {
            foreach (var observer in _listObservers.ToArray())
            {
                observer.OnNext(value);
            }
        }
    }
}