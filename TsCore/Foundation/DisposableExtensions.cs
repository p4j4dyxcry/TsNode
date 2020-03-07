using System;
using System.Linq.Expressions;

namespace TsCore.Foundation
{
    public class Disposable
    {
        private class EmptyDisposable : IDisposable
        {
            internal static readonly EmptyDisposable _instance = new EmptyDisposable();

            public void Dispose()
            {
            }
        }

        internal class DelegateDisposable : IDisposable
        {
            private Action _execute;

            public DelegateDisposable(Action execute)
            {
                _execute = execute;
            }

            public void Dispose()
            {
                _execute?.Invoke();
            }
        }

        public static IDisposable Create(Action action)
            => new DelegateDisposable(action);

        public static IDisposable Empty 
            => EmptyDisposable._instance;

        public static IDisposable UndoDisposable<T>(Action<T> action, T prev, T value)
        {
            action?.Invoke(value);
            return new DelegateDisposable(() =>
            {
                action?.Invoke(prev);
            } );
        }
    }

    public static class DisposableExtensions
    {
        // Dispose時にPropertyのUndoを行うDisposableを作成します
        public static IDisposable ToUndoDisposable<TSubject, TPropertyType>(this TSubject subject, Expression<Func<TSubject, TPropertyType>> action, TPropertyType value)
        {
            var getter = ExpressionPropertyAccessorCache<TSubject>.Get(action);
            var setter = ExpressionPropertyAccessorCache<TSubject>.Set(action);

            var prev = getter.Invoke(subject);

            setter.Invoke(subject,value);
            return new Disposable.DelegateDisposable(() =>
            {
                setter.Invoke(subject,prev);
            });
        }
    }
}