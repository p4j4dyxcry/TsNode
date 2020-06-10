using System;
using System.Linq;
using System.Xml.Serialization;

#if NETCOREAPP3_1
using System.Text.Json.Serialization;
#endif

namespace TsNode.Preset.Models
{
    public class PresetConnection : PresetNotification
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

        public Guid Source { get; set; }
        
        public Guid Dest { get; set; }
        
        private PresetPlug _sourcePlug;
#if NETCOREAPP3_1
        [JsonIgnore]
#endif
        [XmlIgnore]
        public PresetPlug SourcePlug
        {
            get => _sourcePlug;
            set => SetProperty(ref _sourcePlug, value);
        }

        private PresetPlug _destPlug;
#if NETCOREAPP3_1
        [JsonIgnore]
#endif
        [XmlIgnore]
        public PresetPlug DestPlug
        {
            get => _destPlug;
            set => SetProperty(ref _destPlug, value);
        }
        
        public void PreSerialize()
        {
            Source = SourcePlug.Guid;
            Dest = DestPlug.Guid;
        }

        public void Deserialized(Network network)
        {
            var plugs = network.Nodes.SelectMany(x => x.InputPlugs).Concat(network.Nodes.SelectMany(y => y.OutputPlugs)).ToArray();

            SourcePlug = plugs.FirstOrDefault(x => x.Guid == Source);
            DestPlug = plugs.FirstOrDefault(x => x.Guid == Dest);
        }
    }
}