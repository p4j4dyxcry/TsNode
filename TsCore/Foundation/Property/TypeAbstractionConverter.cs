using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TsGui.Foundation.Property
{
    /// <summary>
    /// TSource から TDest にコンバートを行うクラス
    /// 基底クラスの継承関係も考慮してコンバートを行う
    /// </summary>
    public class TypeAbstractionConverter<TSource, TDest>
    {
        private readonly Dictionary<Type, Func<TSource, TDest>> _functions = new Dictionary<Type, Func<TSource, TDest>>();

        public TDest DefaultResult { get; set; } = default(TDest);

        //! 新規タイプを登録
        public void Register<T>(Func<TSource, TDest> func)
        {
            Debug.Assert(func != null);
            Debug.Assert(_functions.ContainsKey(typeof(T)) is false);
            _functions[typeof(T)] = func;
        }

        //! 既存の登録を上書き
        public void Override<T>(Func<TSource, TDest> func)
        {
            Debug.Assert(func != null);
            Debug.Assert(_functions.ContainsKey(typeof(T)) is true);
            _functions[typeof(T)] = func;
        }

        public void Same(Type targetType, Type registeredType)
        {
            Debug.Assert(_functions.ContainsKey(registeredType));

            if (_functions.ContainsKey(targetType) is false)
                _functions[targetType] = _functions[registeredType];
        }

        public TDest Generate(TSource item)
        {
            if (item == null)
                return GetDefaultResult();

            if (_functions.ContainsKey(item.GetType()))
                return _functions[item.GetType()].Invoke(item);

            var baseFunc = FindRegisteredBaseFunction(item.GetType());
            if (baseFunc != null)
                return baseFunc.Invoke(item);

            return GetDefaultResult();
        }

        public bool Any<T>()
        {
            if (_functions.ContainsKey(typeof(T)))
                return true;

            var baseFunc = FindRegisteredBaseFunction(typeof(T));
            if (baseFunc != null)
                return true;

            return false;
        }

        private Func<TSource, TDest> FindRegisteredBaseFunction(Type type)
        {
            var originType = type;
            if (type is null)
                return null;
            type = type.BaseType;

            while (type != null)
            {
                if (_functions.ContainsKey(type))
                {
                    Same(originType, type);
                    return _functions[type];
                }
                type = type.BaseType;
            }

            return null;
        }
      
        protected TDest GetDefaultResult()
        {
            return DefaultResult;
        }
    }
}
