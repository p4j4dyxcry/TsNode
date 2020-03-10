using System;

namespace TsCore.Foundation.Reactive
{
    public class StartWithObserver<T> : ObserverBase<T>
    {
        private readonly Func<T> _defaultValue;
        public StartWithObserver(IObservable<T> observable , Func<T> defaultValue) : base(observable)
        {
            _defaultValue = defaultValue;
            InitializeSubscribe();
        }

        protected override IDisposable SubscribeRaw(IDisposable disposable)
        {
            OnNext(_defaultValue());
            return base.SubscribeRaw(disposable);
        }
    }
}