using System;

namespace TsCore.Foundation.Reactive
{
    public static class ReactiveExtensions
    {
        public static IObservable<T> Do<T>(this IObservable<T> sender, Action<T> action)
        {
            return new DoObserver<T>(sender,action);
        }
        
        public static IObservable<T> Where<T>(this IObservable<T> sender, Func<T, bool> predicate)
        {
            return new WhereObserver<T>(sender,predicate);
        }
        
        public static IObservable<T> Skip<T>(this IObservable<T> sender, int count)
        {
            int i = 0;
            return new WhereObserver<T>(sender, x => i++ > count);
        }
        
        public static IObservable<T> Take<T>(this IObservable<T> sender, int count)
        {
            int i = 0;
            return new WhereObserver<T>(sender, x => i++ < count);
        }
        
        public static IObservable<T> NotNull<T>(this IObservable<T> sender)
        {
            return new WhereObserver<T>(sender,x=> x != null);
        }
        
        public static IObservable<TResult> Select<TSource,TResult>(this IObservable<TSource> sender, Func<TSource,TResult> converter)
        {
            return new SelectObserver<TSource,TResult>(sender,converter);
        }
        
        public static IObservable<TResult> OfType<TSource,TResult>(this IObservable<TSource> sender) where TResult : TSource
        {
            return new OfTypeObserver<TSource,TResult>(sender);
        }
        
        public static IObservable<T> Throttle<T>(this IObservable<T> sender, TimeSpan timeSpan)
        {
            return new ThrottleObserver<T>(sender,timeSpan);
        }
        
        public static IObservable<T> Throttle<T>(this IObservable<T> sender, int timeSpan)
        {
            return new ThrottleObserver<T>(sender,TimeSpan.FromMilliseconds(timeSpan));
        }
        
        public static IObservable<T> StartWith<T>(this IObservable<T> sender, T value = default)
        {
            return new StartWithObserver<T>(sender, () => value);
        }
        
        public static IObservable<T> StartWith<T>(this IObservable<T> sender, Func<T> factory)
        {
            return new StartWithObserver<T>(sender,factory);
        }
        
        public static IObservable<T> Delay<T>(this IObservable<T> sender, TimeSpan timeSpan)
        {
            return new DelayObserver<T>(sender,timeSpan);
        }
        
        public static IObservable<T> Delay<T>(this IObservable<T> sender, int timeSpan)
        {
            return new DelayObserver<T>(sender,TimeSpan.FromMilliseconds(timeSpan));
        }
    }
}