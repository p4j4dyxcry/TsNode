using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TsNode.Controls;
using TsNode.Interface;
using TsNode.Preset;

namespace WpfApp2
{
    public class SimpleCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action<T> _action;
        public SimpleCommand(Action<T> action) => _action = action;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _action.Invoke((T)parameter);
    }

    public class MainWindowViewModel
    {
        public ICommand ConnectedCommand { get; }
        public ICommand ConnectStartCommand { get; }

        public ObservableCollection<INodeDataContext> Nodes {get;set;}
        public ObservableCollection<IConnectionDataContext> Connections { get; set; }

        public MainWindowViewModel()
        {
            Nodes = new ObservableCollection<INodeDataContext>();
            Connections = new ObservableCollection<IConnectionDataContext>();

            ConnectStartCommand = new SimpleCommand<StartCreateConnectionEventArgs>((args) =>
            {
                foreach(var plug in args.SenderPlugs)
                    DeleteConnection(plug);
            });

            ConnectedCommand = new SimpleCommand<CompletedCreateConnectionEventArgs>((args) =>
            {
                DeleteConnection(args.ConnectionDataContext.DestPlug);
                DeleteConnection(args.ConnectionDataContext.SourcePlug);
                Connections.Add(args.ConnectionDataContext);
            });

            CreateNodes();
        }

        // プラグが競合しているコネクションを削除する
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

        private void CreateNodes()
        {
            // 実装サンプル
            var node1 = new PresetNodeViewModel()
            {
                X = 0,
                Y = 50,
                InputPlugs = new ObservableCollection<IPlugDataContext>
                {
                    new PresentPlugViewModel(),
                    new PresentPlugViewModel(),
                },
                OutputPlugs = new ObservableCollection<IPlugDataContext>
                {
                    new PresentPlugViewModel(),
                    new PresentPlugViewModel(),
                }
            };
            var node2 = new PresetNodeViewModel()
            {
                X = 100,
                Y = 500,
                InputPlugs = new ObservableCollection<IPlugDataContext>
                {
                    new PresentPlugViewModel(),
                    new PresentPlugViewModel(),
                },
                OutputPlugs = new ObservableCollection<IPlugDataContext>
                {
                    new PresentPlugViewModel(),
                    new PresentPlugViewModel(),
                }
            };
            var node3 = new PresetNodeViewModel()
            {
                X = 200,
                Y = 50,
                InputPlugs = new ObservableCollection<IPlugDataContext>
                {
                    new PresentPlugViewModel(),
                    new PresentPlugViewModel(),
                },
                OutputPlugs = new ObservableCollection<IPlugDataContext>
                {
                    new PresentPlugViewModel(),
                    new PresentPlugViewModel(),
                }
            };

            Nodes.Add(node1);
            Nodes.Add(node2);
            Nodes.Add(node3);

            var connection = new PresetConnectionViewModel()
            {
                SourcePlug =node1.OutputPlugs.First(),
                DestPlug   = node3.InputPlugs.First(),
            };
            
            Connections.Add(connection);
        }
    }
}
