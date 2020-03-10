using System;

namespace TsCore.Foundation.Reactive
{
    public class OfTypeObserver<T,TU> : ObserveSelectorBase<T,TU>  
        where TU : T
    {
        private static TU Convert(T value)
        {
            if (value is TU tu)
                return tu;
            return default;
        }
        
        public OfTypeObserver(IObservable<T> observable) : base(observable,Convert)
        {
            InitializeSubscribe();
        }

        public override void OnNext(T value)
        {
            if(value is TU )
                base.OnNext(value);
        }
    }
}