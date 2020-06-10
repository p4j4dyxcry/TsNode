using System.Linq;
using System.Windows.Media;

namespace TsNode.Preset.Models
{
    public class NodeService
    {
        private readonly PresetNode _model;
        private readonly Network _network;
        public NodeService(PresetNode node , Network network)
        {
            _model = node;
            _network = network;
        }
        
        public NodeService SetPos(double x, double y)
        {
            _model.X = x;
            _model.Y = y;
            return this;
        }
        
        public NodeService SetColor(Color header)
        {
            _model.HeaderColor = header;
            return this;
        }

        public NodeService SetColors(Color header, Color backGround, Color headerText)
        {
            _model.HeaderColor = header;
            _model.BackGroundColor = backGround;
            _model.HeaderTextColor = headerText;
            return this;
        }

        public NodeService AddInputPlug<T>(string plugName , T defaultValue)
        {
            var plug = new PresetPlug(new Property<T>()
            {
                Name = plugName,
                Value = defaultValue,
            });
            _model.InputPlugs.Add(plug);
            return this;
        }
        
        public NodeService AddOutputPlug<T>(string plugName , T defaultValue)
        {
            var plug = new PresetPlug(new Property<T>()
            {
                Name = plugName,
                Value = defaultValue,
            });
            _model.OutputPlugs.Add(plug);
            return this;
        }

        public void Remove()
        {
            var plugs = _model.InputPlugs.Concat(_model.OutputPlugs).ToArray();

            foreach (var connection in _network.Connections.ToArray())
            {
                if (plugs.Any(x => connection.SourcePlug == x || connection.DestPlug == x))
                {
                    _network.Connections.Remove(connection);
                }
            }
            _network.Nodes.Remove(_model);
        }
    }
}