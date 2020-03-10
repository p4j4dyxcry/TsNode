using System;

namespace TsCore.Foundation.Reactive
{
    public class DoObserver<T> : ObserverBase<T>
    {
        private readonly Action<T> _action;

        public DoObserver(IObservable<T> observable , Action<T> action) : base(observable)
        {
            _action = action;
            InitializeSubscribe();
        }

        public override void OnNext(T value)
        {
            _action(value);
            base.OnNext(value);
        }
    }
}