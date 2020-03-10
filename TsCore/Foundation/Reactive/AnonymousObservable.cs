
using System;

namespace TsCore.Foundation.Reactive
{
    public class AnonymousObservable<T> : IObservable<T>
    {
        private readonly Func<T> _valueGenerator = null;

        private readonly IDisposable _disposable = Disposable.Empty;

        public AnonymousObservable(IDisposable disposable)
        {
            _disposable = disposable ?? Disposable.Empty;
        }
        
        public AnonymousObservable(Func<T> valueGeneratorGenerator)
        {
            _valueGenerator = valueGeneratorGenerator;
        }
        
        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnNext(_valueGenerator());
            
            return _disposable;
        }
    }
}