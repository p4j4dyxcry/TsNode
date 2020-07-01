using System.Collections.ObjectModel;
using System.Windows;
using TsNode.Interface;
using TsNode.Preset.ViewModels;

namespace TsNodeAppBy.NetFramework
{
    public class MainWindowDataContext
    {
        public ObservableCollection<INodeDataContext> Nodes { get; set; }
        public ObservableCollection<IConnectionDataContext> Connections { get; set; }

        public MainWindowDataContext()
        {
            Nodes = new ObservableCollection<INodeDataContext>();
            Connections = new ObservableCollection<IConnectionDataContext>();

            var node1 = new PresetNodeViewModel()
            {
                OutputPlugs = new ObservableCollection<IPlugDataContext>
                {
                    new PresentPlugViewModel(),
                }
            };
            
            var node2 = new PresetNodeViewModel()
            {
                X = 150,
                InputPlugs = new ObservableCollection<IPlugDataContext>
                {
                    new PresentPlugViewModel(),
                },
            };

            Nodes.Add(node1);
            Nodes.Add(node2);

            var connection = new PresetConnectionViewModel()
            {
                SourcePlug = node1.OutputPlugs[0],
                DestPlug = node2.InputPlugs[0],
            };

            Connections.Add(connection);
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowDataContext();
            InitializeComponent();
        }
    }
}