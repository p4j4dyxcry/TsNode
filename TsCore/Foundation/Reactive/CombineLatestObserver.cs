using System;
using System.Collections.Generic;
using System.Linq;

namespace TsCore.Foundation.Reactive
{
    public class CombineLatestObserver<T> : MultiListObserverBase<T>
    {
        public CombineLatestObserver(IObservable<T> observable, params IObservable<T>[] subPrams) : base(observable, subPrams)
        {
            InitializeSubscribe();
        }

        public override void OnNext(T value)
        {
            if (Handlers.All(x => x.HasValue))
            {
                this.OnNext(new List<T>(Handlers.Select(x=>x.Value)));
            }
        }
    }
}