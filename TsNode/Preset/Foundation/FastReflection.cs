using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TsNode.Preset.Foundation
{
    internal static class FastReflection
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

            if (_object.GetType().IsClass == false)
            {
                return new StructAccessor(propertyInfo, PublicOnly);
            }

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
            GetAccessor(_object,property)?.SetValue(_object,value);
        }

        public static object GetProperty(object _object , string property)
        {
            return GetAccessor(_object, property).GetValue( _object);
        }

        public static Type GetPropertyType(object _object, string property)
        {
            return GetAccessor(_object, property).PropertyType;
        }

        public static T GetProperty<T>(object _object, string property)
        {
            return (T)GetAccessor(_object, property).GetValue(_object);
        }

        public static bool ExistsGetter(object _object, string property)
        {
            return GetAccessor(_object, property).HasGetter;
        }

        public static bool ExistsSetter(object _object, string property)
        {
            return GetAccessor(_object, property).HasSetter;
        }

        internal static string GetMemberName<T,TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            return ((MemberExpression)expression.Body).Member.Name;
        }

        private static MethodInfo[] _regisMethodInfo = null;
        private static readonly Dictionary<int, MethodInfo> _methodCache = new Dictionary<int, MethodInfo>();
        
        public static TDelegate CreateDelegate<TDelegate>(object o, MethodInfo method)
        {
            return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), o, method);
        }

        public static void InvokeGenericMethod(object classInstance , Type type, string methodName,params object[] args)
        {
            if (_regisMethodInfo is null)
            {
                _regisMethodInfo = classInstance.GetType()
                    .GetMethods()
                    .Where(x => x.Name == methodName)
                    .Where(x => x.IsGenericMethod)
                    .OrderBy(x => x.GetParameters().Length)
                    .ToArray();
            }

            var hash = type.GetHashCode();

            foreach (var arg in args)
            {
                hash ^= arg.GetType().GetHashCode();
            }

            if (_methodCache.TryGetValue(hash, out var genericMethodInfo) is false)
            {
                _methodCache[hash] =
                    genericMethodInfo =
                        _regisMethodInfo[args.Length - 1].MakeGenericMethod(type);
            }
            Debug.Assert(genericMethodInfo != null);
            genericMethodInfo.Invoke(classInstance, args.Length != 0 ? args : null);
        }
    }
}
