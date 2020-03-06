using System;

namespace TsGui.Foundation.Reactive
{
    public class StartWithObserver<T> : ObserverBase<T>
    {
        private readonly T _defaultValue;
        public StartWithObserver(IObservable<T> observable , T defaultValue) : base(observable)
        {
            _defaultValue = defaultValue;
        }

        protected override IDisposable SubscribeRaw(IDisposable disposable)
        {
            OnNext(_defaultValue);
            return base.SubscribeRaw(disposable);
        }
    }
}