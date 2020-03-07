using System;

namespace TsCore.Foundation.Reactive
{
    public class SelectObserver<T,TU> : ObserveSelectorBase<T,TU>
    {
        public SelectObserver(IObservable<T> observable, Func<T, TU> converter) : base(observable,converter)
        {
            
        }
    }
}