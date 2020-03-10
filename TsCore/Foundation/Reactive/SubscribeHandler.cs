using System;

namespace TsCore.Foundation.Reactive
{
    public class SubscribeHandler<T> : IObserver<T> , IDisposable
    {
        public bool HasValue { get; private set; }
        public T Value { get; private set; }
        
        private IObservable<T> Sender { get; }
        private IObserver<T> _parent;
        private readonly IDisposable _disposable;


        public SubscribeHandler(IObservable<T> sender , IObserver<T> parent)
        {
            Sender = sender;
            _parent = parent;
            _disposable = sender.Subscribe(this);
        }
       
        public void OnNext(T value)
        {
            HasValue = true;
            Value = value;
            _parent.OnNext(value);
        }

        public void OnError(Exception error)
        {
            _parent.OnError(error);
        }

        public void OnCompleted()
        {
            _parent.OnCompleted();
        }

        public void Clear()
        {
            HasValue = false;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}