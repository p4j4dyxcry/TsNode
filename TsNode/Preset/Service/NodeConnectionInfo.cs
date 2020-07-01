using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TsNode.Preset.Models;

namespace TsNode.Preset.Service
{
    public class NodeGroupInfo
    {
        public ConnectionInfo[] Entry { get; }
        
        public ConnectionInfo[] Group { get; }

        private readonly IDictionary<PresetNode, ConnectionInfo> _dictionary;
        
        public int Depth { get; }

        public ConnectionInfo[] GetByDepth(int depth)
        {
            if (depth is 0)
                return Entry;

            var visit = new HashSet<PresetNode>();

            IEnumerable<ConnectionInfo> current = Entry;
            for (int i = 0; i < depth; ++i)
            {
                current = current.SelectMany(x=>GetNext(x,visit));
            }

            return current.ToArray();
        }
        
        private IEnumerable<ConnectionInfo> GetNext(ConnectionInfo info , HashSet<PresetNode> visited)
        {
            foreach (var output in info.Outputs)
            {
                if( visited.Contains(output))
                    continue;
                visited.Add(output);
                yield return _dictionary[output];
            }
        }

        public void Transform(double x, double y)
        {
            foreach (var group in Group)
            {
                group.Node.X += x;
                group.Node.Y += y;
            }
        }

        public NodeGroupInfo(ConnectionInfo[] entry, ConnectionInfo[] group)
        {
            Entry = entry;
            Group = group;

            _dictionary = Group.ToDictionary(x => x.Node, x => x);
            foreach (var info in Group)
            {
                _dictionary[info.Node] = info;
            }
            
            var visit = new HashSet<PresetNode>();

            ConnectionInfo[] current = Entry;
            while (current.Any())
            {
                Depth++;
                current = current.SelectMany(x=>GetNext(x,visit)).ToArray();
            }
            CalcRect();
        }

        public Rect CalcRect()
        {
            var minX = Group.Min(x => x.Node.X);
            var minY = Group.Min(x => x.Node.Y);
            var maxX = Group.Max(x => x.Node.X);
            var maxY = Group.Max(x => x.Node.Y);
            
            return new Rect()
            {
                X = minX,
                Y = minY,
                Width = maxX - minX + 150,
                Height = maxY - minY + 150,
            };
        }
    }

    public class NodeConnectionService
    {
        public IDictionary<PresetNode, ConnectionInfo> Dictionary { get; }

        public ConnectionInfo[] GetInputNodes(PresetNode presetNode)
        {
            return Dictionary[presetNode].Inputs.Select(x=>Dictionary[x]).ToArray();
        }
        
        public ConnectionInfo[] GetOutputsNodes(PresetNode presetNode)
        {
            return Dictionary[presetNode].Outputs.Select(x=>Dictionary[x]).ToArray();
        }

        public NodeConnectionService(IDictionary<PresetNode, ConnectionInfo> dictionary)
        {
            Dictionary = dictionary;
        }
    }
    
    public class ConnectionInfo
    {
        public PresetNode Node { get; }
        public PresetNode[] Inputs { get; }
        public PresetNode[] Outputs { get; }
        public PresetNode[] Connected { get; }

        public ConnectionInfo(PresetNode node , PresetNode[] inputs, PresetNode[] outputs)
        {
            Node = node;
            Inputs = inputs;
            Outputs = outputs;
            Connected = inputs.Concat(outputs).ToArray();
        }
    }

    public class BinaryConnectionInfo
    {
        public int[] Inputs { get; }
        public int[] Outputs { get; }
        public int[] Connected { get; }
        public BinaryConnectionInfo(int[] inputs, int[] outputs)
        {
            Inputs = inputs;
            Outputs = outputs;
            Connected = inputs.Concat(outputs).ToArray();
        }
    }

    public class NodeConnectionCache
    {
        /// <summary>
        /// ノードの接続場を取得する
        /// </summary>
        /// <param name="network"></param>
        /// <returns></returns>
        public static NodeConnectionService BuildConnectionInfos(Network network)
        {
            var dictionary = new Dictionary<PresetNode, ConnectionInfo>();

            var plugToConnection = new Dictionary<PresetPlug, PresetConnection>();
            var plugToNode = new Dictionary<PresetPlug, PresetNode>();

            foreach (var connection in network.Connections)
            {
                plugToConnection[connection.SourcePlug] = connection;
                plugToConnection[connection.DestPlug] = connection;
            }

            foreach (var node in network.Nodes)
            {
                foreach (var plug in node.InputPlugs.Concat(node.OutputPlugs))
                {
                    plugToNode[plug] = node;
                }
            }

            foreach (var node in network.Nodes)
            {
                var inputs = node.InputPlugs
                    .Where( x => plugToConnection.ContainsKey(x))
                    .Select(x => plugToConnection[x])
                    .Select(x => plugToNode[x.SourcePlug])
                    .Distinct()
                    .ToArray();

                var outputs = node.OutputPlugs
                    .Where( x => plugToConnection.ContainsKey(x))
                    .Select(x => plugToConnection[x])
                    .Select(x => plugToNode[x.DestPlug])
                    .Distinct()
                    .ToArray();

                dictionary[node] = new ConnectionInfo(node ,inputs, outputs);
            }

            return new NodeConnectionService(dictionary);
        }

        /// <summary>
        /// ノードの接続情報をindexで取得する
        /// </summary>
        /// <param name="network"></param>
        /// <returns></returns>
        public static BinaryConnectionInfo[] BuildBinaryConnectionInfo(Network network)
        {
            var dictionary = BuildConnectionInfos(network).Dictionary;

            var nodeToIndex = new Dictionary<PresetNode, int>();

            int index = 0;
            foreach (var node in network.Nodes)
                nodeToIndex[node] = index++;

            return dictionary
                .Select( x => new BinaryConnectionInfo(
                    x.Value.Inputs.Select(y => nodeToIndex[y]).ToArray(),
                    x.Value.Outputs.Select(y => nodeToIndex[y]).ToArray()))
                .ToArray();
        }

        public static NodeGroupInfo[] GetConnectedGroup(Network network , IDictionary<PresetNode,ConnectionInfo> dictionary = null)
        {
            var dicionary = dictionary ?? BuildConnectionInfos(network).Dictionary;

            var result = new List<NodeGroupInfo>();

            var founds = new HashSet<PresetNode>();
            
            foreach (var node in network.Nodes)
            {
                var group = FindConnected(node, dicionary, founds).ToArray();
                
                if(group.Length is 0)
                    continue;

                var entry = group.Where(x => dicionary[x].Inputs.Length is 0).ToArray();

                if (entry.Length is 0)
                    entry = new[] {group[0]};
                result.Add(new NodeGroupInfo(entry.Select(x => dicionary[x]).ToArray(),group.Select(x => dicionary[x]).ToArray()));
            }

            return result.ToArray();
        }

        private static IEnumerable<PresetNode> FindConnected(PresetNode node , IDictionary<PresetNode,ConnectionInfo> dictionary ,HashSet<PresetNode> founds)
        {
            if(founds is null)
                founds = new HashSet<PresetNode>();

            if(founds.Contains(node))
                yield break;
            
            founds.Add(node);
            yield return node;

            var info = dictionary[node];

            foreach (var input in info.Inputs)
            {
                var foundItems = FindConnected(input, dictionary, founds).ToArray();
                foreach (var found in foundItems)
                {
                    yield return found;
                }
            }

            foreach (var output in info.Outputs)
            {
                var foundItems = FindConnected(output, dictionary, founds).ToArray();
                foreach (var found in foundItems)
                {
                    yield return found;
                }
            }
        }
    }
}