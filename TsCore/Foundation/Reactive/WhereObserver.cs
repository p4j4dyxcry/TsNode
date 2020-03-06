﻿using System;

namespace TsGui.Foundation.Reactive
{
    public class WhereObserver<T> : ObserverBase<T>
    {
        private readonly Func<T, bool> _predicate;

        public WhereObserver(IObservable<T> observable , Func<T,bool> predicate) : base(observable)
        {
            _predicate = predicate;
        }

        public override void OnNext(T value)
        {
            if(_predicate(value))
                base.OnNext(value);
        }
    }
}