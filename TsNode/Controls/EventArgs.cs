using System;
using System.Collections.Generic;
using System.Windows;
using TsNode.Interface;

namespace TsNode.Controls
{
    public enum SourcePlugType
    {
        Input ,
        Output
    }

    //! イベントの命名は　トリガ　→　動詞　→　名詞　とする

    //! コネクションの生成開始時に発行されるイベント
    public class StartCreateConnectionEventArgs : EventArgs
    {
        public IPlugViewModel[] SenderPlugs { get; }

        public StartCreateConnectionEventArgs(IPlugViewModel[] startPlugs)
        {
            SenderPlugs = startPlugs;
        }
    }

    //! コネクションの生成確定時に発行されるイベント
    public class CompletedCreateConnectionEventArgs : EventArgs
    {
        public IConnectionViewModel ConnectionViewModel { get; }

        public CompletedCreateConnectionEventArgs(IConnectionViewModel connectionViewModel )
        {
            ConnectionViewModel = connectionViewModel;
        }
    }

    //! ノード移動開始時に発行されるイベント
    public class CompletedMoveNodeEventArgs : EventArgs
    {
        public IReadOnlyDictionary<INodeViewModel, Point> InitialNodePoints { get; }
        public IReadOnlyDictionary<INodeViewModel, Point> CompletedNodePoints { get; }

        public CompletedMoveNodeEventArgs(IReadOnlyDictionary<INodeViewModel, Point> initial , IReadOnlyDictionary<INodeViewModel,Point> completed)
        {
            InitialNodePoints = initial;
            CompletedNodePoints = completed;
        }
    }

    //! ノードの座標が更新されたときに発行されるイベント
    public class UpdateNodePointArgs : EventArgs
    {
        public Point Point { get; }

        public UpdateNodePointArgs(double x, double y)
        {
            Point = new Point(x, y);
        }
    }
}
