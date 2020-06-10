using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TsNode.Preset.Models;
#if NETCOREAPP3_1
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace TsNode.Preset
{
    public enum SerializeFormat
    {
        Xml,
        
#if NETCOREAPP3_1
        Json,
# endif
    }
    
    public class NodeEngine
    {
        public Network Network { get; private set; } 
        private Dictionary<string,List<PresetNode>> NodeMap { get; set; } = new Dictionary<string, List<PresetNode>>();

        public NodeEngine()
        {
            Network = new Network();
        }

        public PresetNetworkViewModel BuildViewModel()
        {
            return new PresetNetworkViewModel(Network);
        }

        public string Serialize(SerializeFormat serializeType)
        {
            Network.PreSerialize();
            if (serializeType == SerializeFormat.Xml)
            {
                var xmlSerializer = new XmlSerializer(typeof(Network));
                using (var streamWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(streamWriter, Network);
                    return streamWriter.ToString();
                }
            }
#if NETCOREAPP3_1
            else if (serializeType == SerializeFormat.Json)
            {
                var options = new JsonSerializerOptions();
                options.WriteIndented = true;
                return JsonSerializer.Serialize(Network, options);
            }
#endif
            return string.Empty;
        }

        public void SerializeToFile(string filePath , SerializeFormat serializeFormat)
        {
            File.WriteAllText(filePath,Serialize(serializeFormat));
        }

        public void Deserialize(string stream , SerializeFormat format)
        {
            if (format == SerializeFormat.Xml)
            {
                var xmlSerializer = new XmlSerializer(typeof(Network));
                using (var stringReader = new StringReader(stream))
                {
                    Network = (Network)xmlSerializer.Deserialize(stringReader);
                }
            }
#if NETCOREAPP3_1
            else if (format == SerializeFormat.Json)
            {
                var option = new JsonSerializerOptions();
                option.Converters.Add(new JsonStringEnumConverter());
                Network = JsonSerializer.Deserialize<Network>(stream,option);                
            }
#endif
            Network.Deserialized();
            NodeMap.Clear();
        }

        public void DeserializeFromFile(string filePath , SerializeFormat format)
        {
            var stream = File.ReadAllText(filePath);
            Deserialize(stream,format);
        }
        
        public void Connect(string name1, string name2)
        {
            var node1 = GetOrCreateNodeInternal(name1);
            var node2 = GetOrCreateNodeInternal(name2);

            var plug1 = GetEmptyPlug<string>(node1.OutputPlugs);
            var plug2 = GetEmptyPlug<string>(node2.InputPlugs);
            
            var connection = new PresetConnection()
            {
                SourcePlug = plug1,
                DestPlug = plug2,
            };
            Network.Connections.Add(connection);
        }

        public NodeService GetOrCreateNode(string name)
        {
            return new NodeService(GetOrCreateNodeInternal(name),Network);
        }

        private PresetNode GetNodeInternal(string name)
        {
            if (NodeMap.ContainsKey(name))
            {
                var nodelist = NodeMap[name];
                if (nodelist.Any())
                    return nodelist.First();
            }
            else
            {
                NodeMap[name] = new List<PresetNode>();
            }

            return null;
        }
        
        private PresetNode GetOrCreateNodeInternal(string name)
        {
            var node = GetNodeInternal(name);

            if (node != null)
                return node;

            var id = NodeMap[name].Count;
            node = new PresetNode()
            {
                Name = name,
                Id = id,
            };
            NodeMap[name].Add(node);
            Network.Nodes.Add(node);
            return node;
        }
        
        private PresetPlug GetEmptyPlug<T>(ICollection<PresetPlug> plugs)
        {
            var connectedPlugs = Network.Connections
                .Select(x => x.SourcePlug)
                .Concat(Network.Connections.Select(y => y.DestPlug))
                .Where(x => x.Property.GetGenericType() == typeof(T))
                .Distinct()
                .ToArray();

            foreach (var plug in plugs.Where(x=>x.Property.GetGenericType() == typeof(T)))
            {
                if (connectedPlugs.Contains(plug) is false)
                    return plug;
            }
            
            var newPlug = new PresetPlug(new Property<T>());
            plugs.Add(newPlug);
            return newPlug;
        }
    }
}