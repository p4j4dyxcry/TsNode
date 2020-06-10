using System.Collections.ObjectModel;
using TsNode.Interface;
using TsNode.Preset.Models;

namespace TsNode.Preset
{
    public class PresetNetworkViewModel : PresetNotification
    {
        public ObservableCollection<INodeDataContext> Nodes { get; }
        public ObservableCollection<IConnectionDataContext> Connections { get; }

        public PresetNetworkViewModel(Network model)
        {
            Nodes = model.Nodes.Mapping<PresetNode,INodeDataContext>(x => new PresetNodeViewModel(x));
            Connections = model.Connections.Mapping<PresetConnection,IConnectionDataContext>(x => new PresetConnectionViewModel(x));
        }
    }
}