using System;
using System.Collections.Generic;
using System.Linq;

namespace TsCore.Foundation.Reactive
{
    public class ZipObserver<T> : MultiListObserverBase<T> 
    {
        private readonly IList<IObserver<IList<T>>> _observers = new List<IObserver<IList<T>>>();

        public ZipObserver(IObservable<T> observable, params IObservable<T>[] subPrams) : base(observable, subPrams)
        {
            InitializeSubscribe();
        }

        public override void OnNext(T value)
        {
            if (Handlers.All(x => x.HasValue))
            {
                foreach (var handler in Handlers)
                    handler.Clear();
                
                this.OnNext(new List<T>(Handlers.Select(x=>x.Value)));
            }
        }
    }
}