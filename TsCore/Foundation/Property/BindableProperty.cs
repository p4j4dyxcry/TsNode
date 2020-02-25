using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TsGui.Foundation.Property
{
    public abstract class BindablePropertyBase : Notification, IProperty
    {
        public abstract string Name { get; set; }
        public abstract object Data { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public virtual void RaiseUpdateValue()
        {
            RaisePropertyChanged(nameof(Data));
        }
    }

    public class ReadOnlyProperty : BindablePropertyBase
    {
        public string Value => Data.ToString();

        public ReadOnlyProperty(object data)
        {
            Data = data;
        }

        public override string Name { get; set; }
        public sealed override object Data { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class BindableProperty<T> : BindablePropertyBase
    {
        private readonly Func<T> _generator;
        public event Action<T> OnValueUpdate;

        public T Value
        {
            get => _generator();
            set => OnValueUpdate?.Invoke(value);
        }

        public override object Data
        {
            get => Value;
            set => Value = (T) value;
        }

        private string _name;
        public override string Name
        {
            get => _name;
            set => RaisePropertiesChangedIfSet(ref _name, value);
        }

        public BindableProperty(Func<T> generator)
        {
            _generator = generator;
        }

        public override void RaiseUpdateValue()
        {
            base.RaiseUpdateValue();
            RaisePropertyChanged(nameof(Value));
        }
    }

    public class BindableDoubleProperty : BindableProperty<double>
    {
        public BindableDoubleProperty(Func<double> generator) : base(generator)
        {
        }
    }

    public class BindableIntProperty : BindableProperty<int>
    {
        public BindableIntProperty(Func<int> generator) : base(generator)
        {
        }
    }

    public class BindableStringProperty : BindableProperty<string>
    {
        public BindableStringProperty(Func<string> generator) : base(generator)
        {
        }
    }
    public class BindableBoolProperty : BindableProperty<bool>
    {
        public BindableBoolProperty(Func<bool> generator) : base(generator)
        {
        }
    }

    public class GroupProperty : BindablePropertyBase
    {
        public override string Name { get; set; }
        public override object Data { get; set; }

        public IProperty[] Properties { get;}

        public GroupProperty(IEnumerable<IProperty> properties)
        {
            Properties = properties as IProperty[] ?? properties.ToArray();
        }
    }
    
    public class StructuredProperty : BindablePropertyBase
    {
        public override string Name { get; set; }
        public override object Data { get; set; }

        public IProperty[] Properties { get;}

        public StructuredProperty(IEnumerable<IProperty> properties)
        {
            Properties = properties as IProperty[] ?? properties.ToArray();
        }
    }

    public static class GroupPropertyExtensions
    {
        public static GroupProperty ToGroupProperty(this IEnumerable<IProperty> properties , string name = "")
        {
            return new GroupProperty(properties)
            {
                Name = name,
            };
        }
        
        public static StructuredProperty ToStructuredProperty(this IEnumerable<IProperty> properties , string name = "")
        {
            return new StructuredProperty(properties)
            {
                Name = name,
            };
        }
    }


    public class BindablePropertyFactory
    {
        public static Dictionary<Type,Func<object, BindablePropertyBase>> Converter = new Dictionary<Type, Func<object, BindablePropertyBase>>();

        static BindablePropertyFactory()
        {
            Register<bool>((x) => new BindableBoolProperty((x)));
            Register<double>((x) => new BindableDoubleProperty((x)));
            Register<int>((x) => new BindableIntProperty((x)));
            Register<string>((x) => new BindableStringProperty((x)));
        }

        public static bool Contain<T>()
        {
            return Contain(typeof(T));
        }

        public static bool Contain(Type type)
        {
            return Converter.ContainsKey(type);
        }

        public static void Register<T>(Func< Func<T>, BindablePropertyBase> generator)
        {
            Debug.Assert(Contain<T>() is false, $"{typeof(T).Name} は登録済みです。");

            Converter[typeof(T)] = (x)=> generator.Invoke((Func<T>)(x));
        }

        public BindableProperty<T> Generate<T>(Func<T> func)
        {
            if(Contain<T>())
                return Converter[typeof(T)](func) as BindableProperty<T>;

            return new BindableProperty<T>(func);
        }
    }
}
