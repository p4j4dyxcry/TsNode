using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using TsNode.Preset.Models;
using TsNode.Preset.Service;
using TsNode.Preset.ViewModels;
#if NETCOREAPP3_1
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace TsNode.Preset
{
    /// <summary>
    /// TsNodeをシンプルに扱うための組み込みモデルです。
    /// NodeEngineの利用は必須ではありません。
    /// </summary>
    public class NodeEngine
    {
        public Network Network { get; private set; } 

        /// <summary>
        ///  自動配置の際に利用する仮のノードサイズ
        /// </summary>
        public Size CalculateTempSize { get; private set; } = new Size(150,100);
        
        private Dictionary<string,List<PresetNode>> NodeMap { get; set; } = new Dictionary<string, List<PresetNode>>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NodeEngine()
        {
            Network = new Network();
        }

        /// <summary>
        /// ViewModelを作成します。
        /// この関数で作成されたViewModelは NodeEngineのAPI操作に対して反応し
        /// 自動的にコレクション追加やプロパティ変更通知などが行われるようになります。
        /// </summary>
        /// <returns></returns>
        public PresetNetworkViewModel BuildViewModel()
        {
            return new PresetNetworkViewModel(Network);
        }

        /// <summary>
        /// NetworkModelをシリアライズします。
        /// </summary>
        /// <param name="serializeType"></param>
        /// <returns></returns>
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

        /// <summary>
        /// NetworkModelをファイルにシリアライズして書き出します。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="serializeFormat"></param>
        public void SerializeToFile(string filePath , SerializeFormat serializeFormat)
        {
            File.WriteAllText(filePath,Serialize(serializeFormat));
        }

        /// <summary>
        /// NetworkModelをストリームから構築します。
        /// </summary>
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

        /// <summary>
        /// NetworkModelをファイルから構築します。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="format"></param>
        public void DeserializeFromFile(string filePath , SerializeFormat format)
        {
            var stream = File.ReadAllText(filePath);
            Deserialize(stream,format);
        }
        
        /// <summary>
        /// 引数で指定された名前を持つノードを結合します。
        /// 引数で指定されたノードが無い場合は自動的に作成します。
        /// また、空きプラグがないときは自動的に作成されます。
        /// また、同名ノードが２つ以上ある場合は最初に見つかったものが処理されます。
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="name2"></param>
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

        /// <summary>
        /// ノードを取得します。存在しない場合は新規作成して取得します。
        /// この関数の戻り値は操作用のAPIです。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public NodeService GetOrCreateNode(string name)
        {
            return new NodeService(GetOrCreateNodeInternal(name),Network);
        }

        /// <summary>
        /// ノードを新規作成する
        /// </summary>
        /// <returns></returns>
        public NodeService NewNode()
        {
            var newNode = GetOrCreateNodeInternal(Guid.NewGuid().ToString());
            return new NodeService( newNode , Network );
        }

        /// <summary>
        /// 存在するノードをアルゴリズムに従って自動で配置する
        /// </summary>
        public void AutoArrange()
        {
            var service = NodeConnectionCache.BuildConnectionInfos(Network);
            var groups = NodeConnectionCache.GetConnectedGroup(Network , service.Dictionary);

            var singles = groups.Where(x => x.Group.Length is 1).Select(x=>x.Entry[0].Node).ToArray();
            groups = groups.Where(x => x.Group.Length > 1).ToArray();

            // グループごとの配置はYをズラすようにしています。
            var currentY = 0d;
            foreach (var group in groups)
            {
                ArrangeGroup(group,0,currentY);
                currentY = group.CalcRect().Bottom + CalculateTempSize.Height;
            }
            ArrangeSingleNodes(singles,0,currentY);

        }

        /// <summary>
        /// 接続数が0のまとめます。
        /// </summary>
        /// <param name="singleNodes"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        private void ArrangeSingleNodes(PresetNode[] singleNodes , double startX , double startY)
        {
            var boxSize = Math.Sqrt(singleNodes.Length) * CalculateTempSize.Width;
            var currentX = startX;
            var currentY = startY;
            
            foreach (var single in singleNodes)
            {
                if (currentX >= boxSize)
                {
                    currentX = 0;
                    currentY += CalculateTempSize.Width;
                }
                else
                {
                    currentX += CalculateTempSize.Height;
                }

                single.X = currentX;
                single.Y = currentY;
            }
        }

        /// <summary>
        /// 接続されているノード群を整列します。
        /// </summary>
        /// <param name="group"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        private void ArrangeGroup(NodeGroupInfo group,double startX , double startY)
        {
            for (var i = 0; i < group.Depth; ++i)
            {
                var current = group.GetByDepth(i);
                var y = startY + (CalculateTempSize.Height / 2) * -current.Length;
                foreach (var entry in current)
                {
                    entry.Node.X = startX + i * CalculateTempSize.Width;
                    entry.Node.Y = y;
                    y += CalculateTempSize.Height;
                }
            }
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