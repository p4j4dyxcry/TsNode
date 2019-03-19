using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TsNode.Controls.Node;
using TsNode.Controls.Plug;
using TsNode.Interface;

namespace TsNode.Controls.Connection
{
    /// <summary>
    /// コネクション作成のドラッグコントローラ
    /// </summary>
    public class ConnectionCreateControllerSetupArgs
    {
        //! コネクションの相対座標を計算するためのコントロール
        public IInputElement BaseControl { get; }

        //! マウスイベント
        public MouseEventArgs Args { get; }

        //! ノード一覧 (接続確定を決めるために利用)
        public NodeControl[] Nodes { get; }

        //! ドラッグ開始プラグ(基本的には1つだが拡張性を持たせるために複数利用できるようにしている)
        public PlugControl[] SourcePlugs { get; }

        //! 作成中のコネクションのビューを格納する器 (実コネクションと分離するため)
        public ConnectionItemsControl CreatingConnectionItemsControl { get; }

        //! コネクション作成開始コマンド
        public ICommand ConnectionCreated { get; }

        //! コネクション作成完了コマンド
        public ICommand StartConnectionCreated { get; }

        //! ドラッグ元プラグのタイプ
        public SourcePlugType SourcePlugType { get; }

        public ConnectionCreateControllerSetupArgs(
            IInputElement baseControl,
            MouseEventArgs args,
            NodeControl[] nodes,
            PlugControl[] sourcePlugs,
            ConnectionItemsControl connectionItemsControl,
            ICommand connectionCreated,
            ICommand startConnectionCreated,
            SourcePlugType sourcePlugType)
        {
            Args = args;
            BaseControl = baseControl;
            Nodes = nodes;
            SourcePlugs = sourcePlugs;
            CreatingConnectionItemsControl = connectionItemsControl;
            ConnectionCreated = connectionCreated;
            StartConnectionCreated = startConnectionCreated;
            SourcePlugType = sourcePlugType;
        }

    }

    /// <summary>
    /// ドラッグでコネクションを作成するコントローラ
    /// </summary>
    public class ConnectionCreateController : IDragController
    {
        private readonly IConnectionDataContext[] _connections;
        private readonly IPlugDataContext[] _sourcePlugs;
        private readonly Dictionary<IPlugDataContext, IConnectionDataContext> _plugToConnectionDataContexts;
        private readonly ConnectionItemsControl _connectionItemsControl;
        private readonly NodeControl[] _nodes;
        private readonly ICommand _connectionCreated;
        private readonly SourcePlugType _sourcePlugType;
        private readonly IInputElement _inputElement;

        private bool _isCreated;

        public bool CanDragStart(object sender, MouseEventArgs args)
        {
            return _connections.All(x=>x!=null);
        }

        public void OnDrag(object sender, MouseEventArgs args)
        {
            //! 作成中仮コネクションの作成(1度だけ)
            if (_connectionItemsControl.Items.IsEmpty && _isCreated is false)
            {
                foreach (var plug in _sourcePlugs)
                {
                    var connection = _plugToConnectionDataContexts[plug];
                    if (_sourcePlugType == SourcePlugType.Input)
                        connection.DestPlug = plug;
                    else
                        connection.SourcePlug = plug;
                    _connectionItemsControl.Items.Add(connection);
                }
            }

            foreach (var connection in _connectionItemsControl.FindVisualChildrenWithType<ConnectionShape>())
            {
                var point = args.GetPosition(_inputElement);
                if (_sourcePlugType == SourcePlugType.Output)
                {
                    connection.DestX = point.X;
                    connection.DestY = point.Y;
                }
                if (_sourcePlugType == SourcePlugType.Input)
                {
                    connection.SourceX = point.X;
                    connection.SourceY = point.Y;
                }
            }

            if (args.LeftButton != MouseButtonState.Pressed)
            {
                create_connection();
            }
        }

        public void DragEnd(object sender, MouseEventArgs args)
        {
            create_connection();
        }

        public void Cancel()
        {
            _created();            
        }

        private void _created()
        {
            _connectionItemsControl.Items.Clear();
            _isCreated = true;
        }

        private void create_connection()
        {
            if (_isCreated)
                return;

            var targetPlugs = _nodes
                .SelectMany(x => _sourcePlugType == SourcePlugType.Input ? x.GetOutputPlugs() : x.GetInputPlugs() )
                .Where(x => x.IsMouseOver)
                .ToArray();

            var connectTarget = targetPlugs.FirstOrDefault()?.DataContext as IConnectTarget
                ?? _nodes.FirstOrDefault(x => x.IsMouseOver)?.DataContext as IConnectTarget;

            if (connectTarget != null)
            {
                if (_sourcePlugs.All(x => connectTarget.TryConnect(
                    new ConnectInfo()
                    {
                        Sender = x,
                        SenderType = _sourcePlugType,
                        Connection = _plugToConnectionDataContexts[x]
                    })))
                {
                    // ドラッグ完了コマンド
                    foreach (var dragSourcePlug in _sourcePlugs)
                    {
                        _connectionCreated?.Execute(new CompletedCreateConnectionEventArgs(_plugToConnectionDataContexts[dragSourcePlug]));
                    }
                }
            }

            _created();
        }

        //! コンストラクタ / ドラッグ開始
        public ConnectionCreateController(ConnectionCreateControllerSetupArgs setupArgs)
        {
            _nodes = setupArgs.Nodes;
            _inputElement = setupArgs.BaseControl;

            _sourcePlugs = setupArgs.SourcePlugs
                .Select(x => x.DataContext)
                .OfType<IPlugDataContext>()
                .ToArray();

            _plugToConnectionDataContexts = _sourcePlugs
                .ToDictionary(x => x, x => x.StartConnection());

            _connections = _plugToConnectionDataContexts
                .Select(x => x.Value)
                .ToArray();

            _connectionItemsControl = setupArgs.CreatingConnectionItemsControl;
            _connectionCreated = setupArgs.ConnectionCreated;
            _sourcePlugType = setupArgs.SourcePlugType;

            setupArgs.StartConnectionCreated?.Execute(new StartCreateConnectionEventArgs(_sourcePlugs));
        }
    }
}
