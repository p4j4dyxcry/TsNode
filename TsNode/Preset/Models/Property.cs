
using System;

namespace TsNode.Preset.Models
{
    public class Property : PresetNotification
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value) && string.IsNullOrEmpty(_displayName))
                {
                    RaisePropertyChanged(nameof(DisplayName));
                }
            }
        }

        private string _displayName;
        public string DisplayName
        {
            get => string.IsNullOrEmpty(_displayName) ? Name : _displayName;
            set => SetProperty(ref _displayName, value);
        }

        private object _value;
        public object Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private bool _isReadOnly;

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => SetProperty(ref _isReadOnly, value);
        }

        public virtual Type GetGenericType()
        {
            return typeof(object);
        }
    }

    public class Property<T> : Property
    {
        public new T Value
        {
            get => (T)( base.Value ?? default(T));
            set => base.Value = value;
        }

        public Property()
        {
            Value = default;
        }
        public override Type GetGenericType()
        {
            return typeof(T);
        }
    }
}