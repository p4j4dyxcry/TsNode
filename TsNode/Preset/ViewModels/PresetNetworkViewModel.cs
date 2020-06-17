using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TsNode.Controls;
using TsNode.Interface;
using TsNode.Preset.Foundation;
using TsNode.Preset.Models;

namespace TsNode.Preset.ViewModels
{
    public class PresetNetworkViewModel : PresetNotification
    {
        public ICommand ConnectedCommand { get; }
        public ICommand ConnectStartCommand { get; }
        public ObservableCollection<INodeDataContext> Nodes { get; }
        public ObservableCollection<IConnectionDataContext> Connections { get; }

        public PresetNetworkViewModel(Network model)
        {
            Nodes = model.Nodes.Mapping<PresetNode,INodeDataContext>(x => new PresetNodeViewModel(x));
            Connections = model.Connections.Mapping<PresetConnection,IConnectionDataContext>(x => new PresetConnectionViewModel(x));
            
            ConnectStartCommand = new DelegateCommand<StartCreateConnectionEventArgs>((args) =>
            {
                foreach (var plug in args.SenderPlugs)
                    DeleteConnection(plug);
            });

            ConnectedCommand = new DelegateCommand<CompletedCreateConnectionEventArgs>((args) =>
            {
                DeleteConnection(args.ConnectionDataContext.DestPlug);
                DeleteConnection(args.ConnectionDataContext.SourcePlug);
                Connections.Add(args.ConnectionDataContext);
            });
        }
        private void DeleteConnection(IPlugDataContext plug)
        {
            //!接続済みのプラグだった場合そのコネクション破棄する
            var connected = Connections.FirstOrDefault(x =>
                x.SourcePlug == plug ||
                x.SourcePlug == plug);
            if (connected != null)
            {
                Connections.Remove(connected);
            }
        }
    }
}