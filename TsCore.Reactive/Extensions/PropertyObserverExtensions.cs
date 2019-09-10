using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using TsGui.Foundation;
using TsGui.Mvvm;

namespace TsCore.Reactive.Extensions
{
    /// <summary>
    /// メソッドチェイン用
    /// </summary>
    public static class PropertyChangedObserverExtensions
    {
        /// <summary>
        /// Reactive Extensions の PropertyChangedAsObservable()と等価
        /// </summary>
        public static IObservable<PropertyChangedEventArgs> AsPropertyObserver(
            this INotifyPropertyChanged property)
        {
            return new PropertyObserver(property);
        }

        /// <summary>
        /// Reactive Extensions の CollectionChangedAsObservable()と等価
        /// </summary>
        public static IObservable<NotifyCollectionChangedEventArgs> AsCollectionObserver(
            this INotifyCollectionChanged property)
        {
            return new CollectionObserver(property);
        }

        /// <summary>
        /// Reactive Extensions の ObservePropery()と等価
        /// </summary>
        public static IObservable<TProperty> AsPropertyObserver<TSubject, TProperty>(
            this TSubject subject,
            Expression<Func<TSubject, TProperty>> propertySelector,
            bool isPushCurrentValueAtFirst = true)
            where TSubject : INotifyPropertyChanged
        {
            //! 式木からPropertyのsetterを取り出す
            var accessor = ExpressionPropertyAccessorCache<TSubject>.LookupGet(propertySelector, out var propertyName);
            var isFirst = true;

            //! 初回実行と2回目以降の実行で実装分岐する
            return Observable.Defer(() =>
            {
                var flag = isFirst;
                isFirst = false;

                //! PropertyChangedから目的のPropertyが変更されたかを検証
                var source = subject
                    .AsPropertyObserver()
                    .Where((e => e.PropertyName == propertyName))
                    .Select(_ => accessor(subject));

                //! 初回実行時はPropertyChangedが呼び出されないのでStartWithで現在の値を流す必要がある
                return !(isPushCurrentValueAtFirst & flag)
                    ? source
                    : source.StartWith(accessor(subject));
            });
        }
    }
}