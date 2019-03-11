using System;

namespace TsGui.Operation.Internal
{
    // Getter/Setter
    internal interface IAccessor
    {
        object GetValue(object target);

        void SetValue(object target, object value);
    }

    internal sealed class PropertyAccessor<TTarget, TProperty> : IAccessor
    {
        private readonly Func<TTarget, TProperty> _getter;
        private readonly Action<TTarget, TProperty> _setter;
        public PropertyAccessor(Func<TTarget, TProperty> getter, Action<TTarget, TProperty> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public object GetValue(object target)
        {
            if (_getter != null)
                return _getter((TTarget) target);

            return default;
        }

        public void SetValue(object target, object value)
        {
            _setter?.Invoke((TTarget)target, (TProperty)value);
        }
    }
}
