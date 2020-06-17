using System.Windows;
using System.Windows.Media;
using TsNode.Preset;

namespace NodeEngineSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var engine = new NodeEngine();
            DataContext = engine.BuildViewModel();

            engine.GetOrCreateNode("Node1")
                .SetPos(0, 0)
                .SetColor(Colors.Red);

            engine.GetOrCreateNode("Node2")
                .SetPos(0, 100);
            
            engine.Connect("Node1" , "Node2");
        }
    }
}