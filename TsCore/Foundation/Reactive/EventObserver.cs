using System;

namespace TsCore.Foundation.Reactive
{
    public class EventPattern<TEventArgs>
    {
        public object Sender { get; }
        public TEventArgs EventArgs { get; }
        
        public EventPattern(object sender, TEventArgs eventArgs)
        {
            Sender = sender;
            EventArgs = eventArgs;
        }
    }

    public class EventPatternObserver<TEventHandler,TEventArgs> : ObserverProduct<EventPattern<TEventArgs>, EventPattern<TEventArgs>>
    {
        private readonly Action<TEventHandler> _removeHandler;
        private readonly TEventHandler _eventHandler;
        private readonly Func<Action<object,TEventArgs>,TEventHandler> _conversion;
        
        public EventPatternObserver(Action<TEventHandler> addHandler , Action<TEventHandler> removeEventHandler) : base(x=>x)
        {
            _eventHandler = GetHandler(OnNext);
            _removeHandler = removeEventHandler;
            addHandler(_eventHandler);
        }
        
        public EventPatternObserver(Func<Action<object,TEventArgs>,TEventHandler> conversion, Action<TEventHandler> addHandler , Action<TEventHandler> removeEventHandler) : base(x=>x)
        {
            _conversion = conversion;
            _eventHandler = GetHandler(OnNext);
            _removeHandler = removeEventHandler;

            addHandler(_eventHandler);
        }
        
        private TEventHandler GetHandler(Action<EventPattern<TEventArgs>> onNext)
        {
            TEventHandler handler;

            if (_conversion == null)
            {
                Action<object, TEventArgs> h = (sender, eventArgs) => onNext(new EventPattern<TEventArgs>(sender, eventArgs));
                handler = FastReflection.CreateDelegate<TEventHandler>(h, typeof(Action<object, TEventArgs>).GetMethod(nameof(Action<object, TEventArgs>.Invoke)));
            }
            else
            {
                handler = _conversion((sender, eventArgs) => onNext(new EventPattern<TEventArgs>(sender, eventArgs)));
            }

            return handler;
        }   
                
        protected override void Dispose(bool disposed)
        {
            if (disposed is false)
            {
                _removeHandler(_eventHandler);
            }
        } 
    }
    
    public class EventObserver<TEventHandler , TEventArgs>: ObserverProduct<TEventArgs,TEventArgs>
    {
        private readonly Action<TEventHandler> _removeHandler;
        private readonly TEventHandler _eventHandler;
        private readonly Func<Action<TEventArgs>, TEventHandler> _conversion;

        public EventObserver(Action<TEventHandler> addHandler , Action<TEventHandler> removeEventHandler) : base(x=>x)
        {
            _eventHandler = GetHandler(OnNext);
            _removeHandler = removeEventHandler;
            addHandler(_eventHandler);
        }
        
        public EventObserver(Func<Action<TEventArgs>, TEventHandler> conversion, Action<TEventHandler> addHandler , Action<TEventHandler> removeEventHandler) : base(x=>x)
        {
            _conversion = conversion;
            _eventHandler = GetHandler(OnNext);
            _removeHandler = removeEventHandler;

            addHandler(_eventHandler);
        }

        private TEventHandler GetHandler(Action<TEventArgs> onNext)
        {
            TEventHandler handler;

            if (_conversion is null)
            {
                handler = FastReflection.CreateDelegate<TEventHandler>(onNext, typeof(Action<TEventArgs>).GetMethod(nameof(Action<TEventArgs>.Invoke)));
            }
            else
            {
                handler = _conversion(onNext);
            }

            return handler;
        }
        
        protected override void Dispose(bool disposed)
        {
            if (disposed is false)
            {
                _removeHandler(_eventHandler);
            }
        } 
    }
}