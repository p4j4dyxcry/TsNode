using System.Collections.Generic;
using System.Numerics;
using TsNode.Interface;
using TsNode.Preset.Foundation;
using TsNode.Preset.Models;

namespace TsNode.Preset.ViewModels
{
    /// <summary>
    /// 実装サンプル
    /// </summary>
    public class PresentPlugViewModel : PresetNotification , IPlugDataContext
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => RaisePropertyChangedIfSet(ref _name, value);
        }
        
        public PresetPlug Plug { get; set; }
        
        public virtual IConnectionDataContext StartConnectionOverride()
        {
            return new PresetConnectionViewModel();
        }

        public IConnectionDataContext StartConnection()
        {
            return StartConnectionOverride();
        }

        public PresentPlugViewModel()
        {
            
        }

        public virtual bool TryConnect(ConnectInfo connectInfo)
        {
            if (connectInfo.Connection.SourcePlug is null)
                connectInfo.Connection.SourcePlug = this;
            else if(connectInfo.Connection.DestPlug is null)
                connectInfo.Connection.DestPlug = this;
            
            return true;
        }
    }

    public class PresentPlugViewModel<T> : PresentPlugViewModel
    {
        private T _value;
        public T Value
        {
            get => _value;
            set => RaisePropertyChangedIfSet(ref _value, value);
        }

        public PresentPlugViewModel(Property plug) 
        {
            this.BindModel(plug);
        }

        public override bool TryConnect(ConnectInfo connectInfo)
        {
            if (connectInfo.Connection.SourcePlug is null && connectInfo.Connection.DestPlug is PresentPlugViewModel<T>)
                connectInfo.Connection.SourcePlug = this;
            else if(connectInfo.Connection.DestPlug is null && connectInfo.Connection.SourcePlug is PresentPlugViewModel<T>)
                connectInfo.Connection.DestPlug = this;
            
            return true;
        }
    }

    public static class PlugViewModelService
    {
        private static readonly Dictionary<PresetPlug, IPlugDataContext> _cache = new Dictionary<PresetPlug, IPlugDataContext>();

        public static PresetPlug ContextToModel(IPlugDataContext dataContext)
        {
            if (dataContext is PresentPlugViewModel presetPlug)
                return presetPlug.Plug;

            return null;
        }
        
        public static IPlugDataContext GetOrCreate(PresetPlug plug)
        {
            if (_cache.ContainsKey(plug))
                return _cache[plug];
            PresentPlugViewModel result = null;
            switch (plug.Property)
            {
                case Property<int> property:
                    result = new IntPlugViewModel(property);
                    break;
                case Property<float> property:
                    result =  new FloatPlugViewModel(property);
                    break;
                case Property<Vector3> property:
                    result =  new Vector3PlugViewModel(property);
                    break;
                case Property<string> property:
                    result =  new StringPlugViewModel(property);
                    break;
                case Property<bool> property:
                    result =  new BoolPlugViewModel(property);
                    break;
                default:
                    result =  new PresentPlugViewModel();
                    break;
            }

            result.Plug = plug;
            return _cache[plug] = result;
        }
    }

    // 特殊化
    public class IntPlugViewModel : PresentPlugViewModel<int>
    {
        public IntPlugViewModel(Property<int> plug) : base(plug)
        {
        }
    }
    public class FloatPlugViewModel : PresentPlugViewModel<float>
    {
        public FloatPlugViewModel(Property<float> plug) : base(plug)
        {
        }
    }

    public class StringPlugViewModel : PresentPlugViewModel<string>
    {
        public StringPlugViewModel(Property<string> plug) : base(plug)
        {
        }
    }
    
    public class Vector3PlugViewModel : PresentPlugViewModel<Vector3>
    {
        public Vector3PlugViewModel(Property<Vector3> plug) : base(plug)
        {
        }
    }
    
    public class BoolPlugViewModel : PresentPlugViewModel<bool>
    {
        public BoolPlugViewModel(Property<bool> plug) : base(plug)
        {
        }
    }
}