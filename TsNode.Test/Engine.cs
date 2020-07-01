using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Media;
using TsNode.Preset;
using TsNode.Preset.Models;
using Xunit;

namespace TsNode.Test
{
    public class Engine
    {
        [Fact]
        public void CreateTest()
        {
            var engine = new NodeEngine();
            
            engine.GetOrCreateNode("Node1")
                .SetPos(150, 200)
                .SetColor(Colors.DarkRed);

            engine.GetOrCreateNode("Node2")
                .SetPos(400, 500)
                .SetColor(Colors.Blue);

            var nodes = engine.Network.Nodes;
            Assert.Equal(2, nodes.Count );
            Assert.Equal("Node1",nodes[0].Name);
            Assert.Equal("Node2",nodes[1].Name);
            Assert.Equal(150,nodes[0].X);
            Assert.Equal(500,nodes[1].Y);
            Assert.Equal(Colors.DarkRed , nodes[0].HeaderColor);
            Assert.Equal(Colors.Blue , nodes[1].HeaderColor);
        }
        [Fact]
        public void BuildViewModelTest()
        {
            var engine = new NodeEngine();
            var vm = engine.BuildViewModel();

            engine.GetOrCreateNode("Node1");
            engine.GetOrCreateNode("Node2");
            engine.Connect("Node1","Node2");
            engine.Connect("Node2","Node1");

            // engineAPIにより ViewModelの数が更新されていることを確認する
            Assert.Equal(2 , vm.Nodes.Count);
            Assert.Equal(2 , vm.Connections.Count);
            Assert.Equal(4 , vm.Nodes.SelectMany(x=>x.GetInputPlugs()).Concat(vm.Nodes.SelectMany(y=>y.GetOutputPlugs())).Count());
            
            engine.GetOrCreateNode("Node1")
                .Remove();
            
            // 削除が反映されることを確認
            Assert.Equal(1 , vm.Nodes.Count);
            Assert.Equal(0 , vm.Connections.Count);
            Assert.Equal(2 , vm.Nodes.SelectMany(x=>x.GetInputPlugs()).Concat(vm.Nodes.SelectMany(y=>y.GetOutputPlugs())).Count());
            
            engine.GetOrCreateNode("Node2")
                .Remove();

            var network = engine.Network;
            
            // モデルも消えていることを確認
            Assert.Equal(0 , network.Nodes.Count);
            Assert.Equal(0 , network.Connections.Count);
            Assert.Equal(0 , network.Nodes.SelectMany(x=>x.InputPlugs).Concat(network.Nodes.SelectMany(y=>y.OutputPlugs)).Count());
        }

        [Theory]
        [InlineData(SerializeFormat.Json)]
        [InlineData(SerializeFormat.Xml)]
        public void SerializeTest(SerializeFormat format)
        {
            var engine = new NodeEngine();
            string stream = null;

            engine.GetOrCreateNode("Node1")
                .SetColor(Colors.Coral)
                .SetPos(150, 200);

            engine.GetOrCreateNode("Node2");
            
            engine.Connect("Node1","Node2");

            stream = engine.Serialize(format);

            var engine2 = new NodeEngine();
            engine2.Deserialize(stream,format);
            
            Assert.Equal(engine.Network.Nodes[0].Guid , engine2.Network.Nodes[0].Guid);
            Assert.Equal(engine.Network.Nodes[0].Y , engine2.Network.Nodes[0].Y);
            Assert.Equal(engine.Network.Nodes[0].HeaderColor , engine2.Network.Nodes[0].HeaderColor);
            
            Assert.Equal(engine.Network.Nodes.Count , engine2.Network.Nodes.Count);
            Assert.Equal(engine.Network.Connections.Count , engine2.Network.Connections.Count);
            Assert.Equal(engine.Network.Nodes.SelectMany(x=>x.InputPlugs).Count() , engine2.Network.Nodes.SelectMany(x=>x.InputPlugs).Count());
            Assert.Equal(engine.Network.Nodes.SelectMany(x=>x.OutputPlugs).Count() , engine2.Network.Nodes.SelectMany(x=>x.OutputPlugs).Count());
            
        }

        [Fact]
        public void SerializeTest2()
        {
            var nodeEngine = new NodeEngine();
            nodeEngine.GetOrCreateNode("Root")
                .SetColor(Colors.Gold)
                .AddInputPlug("Plug1", 10)
                .AddInputPlug("Plug2", "string")
                .AddInputPlug("Plug4", 50f)
                .AddInputPlug("Plug5", 100d)
                .AddInputPlug("Plug6", false)
                .AddOutputPlug("Plug7", 20)
                .AddOutputPlug("Plug8", "string2")
                .AddOutputPlug("Plug9", 70f)
                .AddOutputPlug("Plug10", 120d)
                .AddOutputPlug("Plug11", true)
                .AddInputPlug("Plug12", new Vector3(0, 1, 2));

            nodeEngine.Connect("Root","Child1");
            
            // 自動配置のテスト
            nodeEngine.AutoArrange();
            
            nodeEngine.SerializeToFile("output.json",SerializeFormat.Json);
            nodeEngine.SerializeToFile("output.xml",SerializeFormat.Xml);
            
            File.Delete("output.json");
            File.Delete("output.xml");
        }
    }
}