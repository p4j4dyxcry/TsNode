using System;
using System.Threading.Tasks;

namespace TsCore.Foundation.Reactive
{
    public class ThrottleObserver<T> : ObserverBase<T>
    {
        private readonly TimeSpan _timeSpan;
        private bool _queryTask = false;
        private T _lastValue;
        private DateTime _lastUpdate;
        
        public ThrottleObserver(IObservable<T> observable , TimeSpan timeSpan) : base(observable)
        {
            _timeSpan = timeSpan;
        }
        
        public override async void OnNext(T value)
        {
            _lastValue = value;
            _lastUpdate = DateTime.Now;
            if (_queryTask)
            {
                return;
            }

            _queryTask = true;
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1);
                    
                    if(DateTime.Now + _timeSpan > _lastUpdate)
                        break;
                }
            });
            base.OnNext(_lastValue);
            _queryTask = false;
        }
    }
}