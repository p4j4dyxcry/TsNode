using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace TsGui.Operation.Internal
{
    public static class FastReflection
    {        
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, IAccessor>> Cache 
            = new ConcurrentDictionary<Type, ConcurrentDictionary<string, IAccessor>>();

        public static bool PublicOnly { get; set; } = true;

        private static IAccessor MakeAccessor(object _object, string propertyName)
        {
            var propertyInfo = _object.GetType().GetProperty(propertyName,
                BindingFlags.NonPublic | 
                BindingFlags.Public |
                BindingFlags.Instance);

            if (propertyInfo == null)
                return null;

            var getInfo = propertyInfo.GetGetMethod(PublicOnly is false);
            var setInfo = propertyInfo.GetSetMethod(PublicOnly is false);

            var getterDelegateType = typeof(Func<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            var getter = getInfo != null ? Delegate.CreateDelegate(getterDelegateType, getInfo) : null;

            var setterDelegateType = typeof(Action<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            var setter = setInfo != null? Delegate.CreateDelegate(setterDelegateType, setInfo) : null;

            var accessorType = typeof(PropertyAccessor<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (IAccessor)Activator.CreateInstance(accessorType, getter, setter);
        }

        private static IAccessor GetAccessor(object _object, string propertyName)
        {
            var accessors = Cache.GetOrAdd(_object.GetType(), x => new ConcurrentDictionary<string, IAccessor>());
            return accessors.GetOrAdd(propertyName, x => MakeAccessor(_object, propertyName));
        }

        public static void SetProperty(object _object, string property, object value)
        {
            GetAccessor(_object,property).SetValue(_object,value);
        }

        public static object GetProperty(object _object , string property)
        {
            return GetAccessor(_object, property).GetValue( _object);
        }
        internal static string GetMemberName<T,TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            return ((MemberExpression)expression.Body).Member.Name;
        }
    }
}
