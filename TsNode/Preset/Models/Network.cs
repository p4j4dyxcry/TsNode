using System.Collections.ObjectModel;

namespace TsNode.Preset.Models
{
    /// <summary>
    /// ノードグラフを管理するモデルです。
    /// </summary>
    public class Network : PresetNotification
    {
        public ObservableCollection<PresetNode> Nodes { get; set; } = new ObservableCollection<PresetNode>();
        public ObservableCollection<PresetConnection> Connections { get; set; } = new ObservableCollection<PresetConnection>();

        /// <summary>
        /// シリアライズ前に実行されます。
        /// 一部データをシリアライズ用に変換します。
        /// </summary>
        public void PreSerialize()
        {
            foreach (var node in Nodes)
            {
                node.PreSerialize();
            }
            
            foreach (var connection in Connections)
            {
                connection.PreSerialize();
            }
        }

        /// <summary>
        /// デシリアライズ後に実行されます。
        /// 一部データを実データに変換します。
        /// </summary>
        public void Deserialized()
        {
            foreach (var node in Nodes)
            {
                node.Deserialized();
            }
            
            foreach (var connection in Connections)
            {
                connection.Deserialized(this);
            }
        }
    }
}