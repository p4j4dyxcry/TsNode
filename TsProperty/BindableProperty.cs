using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace TsProperty
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

    public class ReadOnlyProperty : Notification, IProperty
    {
        public string Name { get; set; }

        public string Value => Data.ToString();

        public object Data { get; set; }

        public ReadOnlyProperty(object data)
        {
            Data = data;
        }

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
    
    public class BindablePropertyFactory
    {
        public static Dictionary<Type,Func<object, BindablePropertyBase>> Converter = new Dictionary<Type, Func<object, BindablePropertyBase>>();

        static BindablePropertyFactory()
        {
            Converter[typeof(double)] = (x) => new BindableDoubleProperty(((Func<double>)(x)));
            Converter[typeof(int)   ] = (x) => new BindableIntProperty(((Func<int>) (x)));
            Converter[typeof(string)] = (x) => new BindableStringProperty(((Func<string>)(x)));
            Converter[typeof(bool)  ] = (x) => new BindableBoolProperty(((Func<bool>)(x)));
        }

        public BindableProperty<T> Generate<T>(Func<T> func)
        {
            if(Converter.ContainsKey(typeof(T)) is false)
                return new BindableProperty<T>(func);
            return Converter[typeof(T)](func) as BindableProperty<T>;
        }
    }
}
