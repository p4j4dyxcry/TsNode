
using System;
using System.Collections.Generic;

namespace TsCore.Foundation.Reactive
{
    public static class Observable
    {
        public static IObservable<T> Return<T>(T value)
        {
            return new AnonymousObservable<T>( ()=>value);
        }
        
        public static IObservable<T> Defer<T>(Func<IObservable<T>> value)
        {
            return value();
        }

        public static IObservable<T> Merge<T>(IObservable<T> value , params IObservable<T>[] values)
        {
            return new MergeObserver<T>(value,values);
        }
        
        public static IObservable<IList<T>> Zip<T>(IObservable<T> value , params IObservable<T>[] values)
        {
            return new ZipObserver<T>(value,values);
        }
        
        public static IObservable<IList<T>> CombineLatest<T>(IObservable<T> value , params IObservable<T>[] values)
        {
            return new CombineLatestObserver<T>(value,values);
        }

        public static IObservable<TEventArgs> FromEvent<TEventHandler, TEventArgs>(Action<TEventHandler> addHandler, Action<TEventHandler> removeHandler)
        {
            return new EventObserver<TEventHandler,TEventArgs>(addHandler , removeHandler);
        }
        
        public static IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler )
        {
            return new EventPatternObserver<EventHandler<TEventArgs>,TEventArgs>(addHandler , removeHandler);
        }
    }
}