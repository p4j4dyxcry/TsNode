using System;
using System.Reflection;

namespace TsNode.Preset.Foundation
{
    internal interface IAccessor
    {
        object GetValue(object target);

        void SetValue(object target, object value);

        bool HasGetter { get; }
        bool HasSetter { get; }

        Type PropertyType { get; }
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

        public bool HasGetter => (_getter != null);
        public bool HasSetter => (_setter != null);

        public Type PropertyType { get; } = typeof(TProperty);
    }


    internal sealed class StructAccessor : IAccessor
    {
        private readonly PropertyInfo _propertyInfo;
        public StructAccessor(PropertyInfo propertyInfo , bool publicOnly)
        {
            _propertyInfo = propertyInfo;
            HasGetter = propertyInfo.GetGetMethod(publicOnly is false) != null;
            HasSetter = propertyInfo.GetSetMethod(publicOnly is false) != null;
            PropertyType = _propertyInfo.PropertyType;
        }

        public object GetValue(object target)
        {
            return _propertyInfo.GetValue(target);
        }

        public void SetValue(object target, object value)
        {
            _propertyInfo.SetValue(target,value);
        }

        public bool HasGetter { get; }

        public bool HasSetter { get; }

        public Type PropertyType { get; }
    }
}
