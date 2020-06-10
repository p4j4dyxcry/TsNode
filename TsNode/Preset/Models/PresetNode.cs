using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Xml.Serialization;

#if NETCOREAPP3_1
using System.Text.Json.Serialization;
#endif

namespace TsNode.Preset.Models
{
    public interface IHasGuid
    {
        Guid Guid { get; set; }
    }
    
    public class PresetNode : PresetNotification , IHasGuid
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private double _x;
        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        private double _y;

        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        private Color _header = (Color)ColorConverter.ConvertFromString("#2c3e50");
#if NETCOREAPP3_1
        [JsonIgnore]
#endif
        [XmlIgnore]
        public Color HeaderColor
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        private Color _background = (Color)ColorConverter.ConvertFromString("#bdc3c7");
#if NETCOREAPP3_1
        [JsonIgnore]
#endif
        [XmlIgnore]
        public Color BackGroundColor
        {
            get => _background;
            set => SetProperty(ref _background, value);
        }

        private Color _headerTextColor= (Color)ColorConverter.ConvertFromString("#ecf0f1");
#if NETCOREAPP3_1
        [JsonIgnore]
#endif
        [XmlIgnore]
        public Color HeaderTextColor
        {
            get => _headerTextColor;
            set => SetProperty(ref _headerTextColor, value);
        }
        
        // シリアライズ用、ロジックとしての利用禁止
        public ObservableCollection<PlugPropertyInfo> IOProperties { get; set; } = new ObservableCollection<PlugPropertyInfo>();
        public string HeaderColorHex { get; set; }
        public string BackGroundColorHex { get; set; }
        public string HeaderTextColorHex { get; set; }

#if NETCOREAPP3_1
        [JsonIgnore]
#endif
        [XmlIgnore]
        public ObservableCollection<PresetPlug> InputPlugs { get; set; } = new ObservableCollection<PresetPlug>();

#if NETCOREAPP3_1
        [JsonIgnore]
#endif
        [XmlIgnore]
        public ObservableCollection<PresetPlug> OutputPlugs { get; set; } = new ObservableCollection<PresetPlug>();

        public void PreSerialize()
        {
            IOProperties.Clear();
            foreach (var plug in InputPlugs)
                IOProperties.Add(PlugPropertyInfo.Serialize(plug,IOType.Input));
            foreach (var plug in OutputPlugs)
                IOProperties.Add(PlugPropertyInfo.Serialize(plug,IOType.Output));
            HeaderColorHex     = HeaderColor.ToString();
            BackGroundColorHex = BackGroundColor.ToString();
            HeaderTextColorHex = HeaderTextColor.ToString();
        }

        public void Deserialized()
        {
            foreach (var property in IOProperties)
            {
                if (property.IoType == IOType.Input)
                {
                    InputPlugs.Add(property.CreatePlug());
                }
                if (property.IoType == IOType.Output)
                {
                    OutputPlugs.Add(property.CreatePlug());
                }
            }
            HeaderColor     = (Color)ColorConverter.ConvertFromString(HeaderColorHex);
            BackGroundColor = (Color)ColorConverter.ConvertFromString(BackGroundColorHex);
            HeaderTextColor = (Color)ColorConverter.ConvertFromString(HeaderTextColorHex);
        }
    }
}