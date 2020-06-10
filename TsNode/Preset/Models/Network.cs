using System.Collections.ObjectModel;

namespace TsNode.Preset.Models
{
    public class Network : PresetNotification
    {
        public ObservableCollection<PresetNode> Nodes { get; set; } = new ObservableCollection<PresetNode>();
        public ObservableCollection<PresetConnection> Connections { get; set; } = new ObservableCollection<PresetConnection>();

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