using System.Linq;
using System.Windows.Media;

namespace TsNode.Preset.Models
{
    /// <summary>
    /// ノード操作用のAPI
    /// </summary>
    public class NodeService
    {
        private readonly PresetNode _model;
        private readonly Network _network;
        public NodeService(PresetNode node , Network network)
        {
            _model = node;
            _network = network;
        }
        
        /// <summary>
        /// 座標を指定します。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public NodeService SetPos(double x, double y)
        {
            _model.X = x;
            _model.Y = y;
            return this;
        }
        
        /// <summary>
        /// ヘッダー色を変更します。
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public NodeService SetColor(Color header)
        {
            _model.HeaderColor = header;
            return this;
        }

        /// <summary>
        /// ヘッダー色、背景色、前景色をまとめて変更します。
        /// </summary>
        /// <param name="header"></param>
        /// <param name="backGround"></param>
        /// <param name="headerText"></param>
        /// <returns></returns>
        public NodeService SetColors(Color header, Color backGround, Color headerText)
        {
            _model.HeaderColor = header;
            _model.BackGroundColor = backGround;
            _model.HeaderTextColor = headerText;
            return this;
        }

        /// <summary>
        /// 任意の入力プラグTを追加します。
        /// </summary>
        /// <param name="plugName"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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
        
        /// <summary>
        /// 任意の出力プラグTを追加します。
        /// </summary>
        /// <param name="plugName"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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
        
        /// <summary>
        /// ノードを削除します。関連するコネクション等も削除されます。
        /// </summary>
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

        public string Handle => _model.Name;
    }
}