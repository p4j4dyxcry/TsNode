using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TsGui;
using TsGui.Operation;
using TsGui.Operation.Internal;

namespace TsProperty
{
    public class PropertyBuilder : IPropertyBuilder
    {
        protected object Object { get; }
        private readonly IList<IProperty> _properties = new List<IProperty>();
        private IOperationController _operationController;

        public PropertyBuilder(object @object)
        {
            Object = @object;

            if (Object is INotifyPropertyChanged propertyChanged)
            {
                propertyChanged.PropertyChanged += (s, e) =>
                {
                    foreach (var p in _properties.OfType<BindablePropertyBase>())
                    {
                        if(p.Name == e.PropertyName)
                            p.RaiseUpdateValue();
                    }
                };
            }
        }

        public IPropertyBuilder OperationController(IOperationController controller)
        {
            _operationController = controller;
            return this;
        }

        public IPropertyBuilder Register<T>(string propertyName)
        {
            if (FastReflection.ExistsSetter(Object, propertyName) is false)
                 RegisterReadOnly(() => FastReflection.GetProperty<T>(Object, propertyName));
            else
                 Register(
                ()=>FastReflection.GetProperty<T>(Object, propertyName),
                (x)=>
                {
                    FastReflection.SetProperty(Object, propertyName, x);
                });

            _properties[_properties.Count - 1].Name = propertyName;

            return this;
        }

        public IPropertyBuilder Register<T>(Func<T> getter, Action<T> setter)
        {
            var property = new BindablePropertyFactory().Generate(getter);

            if (setter is null)
                return RegisterReadOnly(getter);

            //! Setter を作成します
            property.OnValueUpdate += (newValue) =>
            {
                var oldValue = property.Value;
                if (Equals(oldValue, newValue))
                    return;

                if (_operationController != null)
                {
                    //! controllerがある場合はOperationとして実行し Undo をサポートする
                    var operationBuilder = new OperationBuilder();
                    var operation = operationBuilder.MakeThrottle(setter, newValue, oldValue,setter.GetHashCode(),TimeSpan.MaxValue)
                        .PostEvent(property.RaiseUpdateValue)
                        .Name($"{property.Name} New:{newValue}")
                        .Build()
                        .ExecuteTo(_operationController);
                }
                else
                {
                    //! controllerが無い場合は随時実行する
                    setter.Invoke(newValue);
                    property.RaiseUpdateValue();
                }
            };

            _properties.Add(property);
            return this;
        }

        public IPropertyBuilder RegisterReadOnly<T>(Func<T> getter)
        {
            _properties.Add(new ReadOnlyProperty(getter()));
            return this;
        }

        public IEnumerable<IProperty> Build()
        {
            return _properties.ToArray();
        }
    }

    public class ReflectionPropertyBuilder : PropertyBuilder
    {
        private static readonly Dictionary<Type, PropertyInfo[]> _cache = new Dictionary<Type, PropertyInfo[]>();

        public ReflectionPropertyBuilder(object @object):base(@object)
        {
        }

        public ReflectionPropertyBuilder GenerateProperties()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            var objectType = Object.GetType();
            if (_cache.TryGetValue(objectType, out var properties) is false)
                properties = _cache[objectType] = objectType.GetProperties(bindingFlags);

            Debug.Assert(properties != null);

            var propertyNames = properties.Select(x => x.Name).ToArray();

            foreach (var propertyName in propertyNames)
            {
                var propertyType = FastReflection.GetPropertyType(Object, propertyName);
                FastReflection.InvokeGenericMethod(this, propertyType, nameof(Register),propertyName);
            }
            return this;
        }
    }
}
