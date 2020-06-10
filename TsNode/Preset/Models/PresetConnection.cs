using System;
using System.Linq;
using System.Xml.Serialization;

#if NETCOREAPP3_1
using System.Text.Json.Serialization;
#endif

namespace TsNode.Preset.Models
{
    /// <summary>
    /// 結線のモデルです。
    /// </summary>
    public class PresetConnection : PresetNotification
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

        // シリアライズ用 XMLSerializer用にプロパティを公開していますが操作禁止です。
        // シリアライズ直前に値が設定されます。
        public Guid Source { get; set; }
        public Guid Dest { get; set; }

        /// <summary>
        /// 入力プラグです。
        /// </summary>
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

        /// <summary>
        /// 出力プラグです。
        /// </summary>
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
        
        /// <summary>
        /// シリアライズ前処理
        /// </summary>
        public void PreSerialize()
        {
            Source = SourcePlug.Guid;
            Dest = DestPlug.Guid;
        }

        /// <summary>
        /// デシリアライズ後処理
        /// </summary>
        /// <param name="network"></param>
        public void Deserialized(Network network)
        {
            var plugs = network.Nodes.SelectMany(x => x.InputPlugs).Concat(network.Nodes.SelectMany(y => y.OutputPlugs)).ToArray();

            SourcePlug = plugs.FirstOrDefault(x => x.Guid == Source);
            DestPlug = plugs.FirstOrDefault(x => x.Guid == Dest);
        }
    }
}