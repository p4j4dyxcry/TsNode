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

    //! コネクションの生成開始時に発行されるイベント
    public class StartCreateConnectionEventArgs : EventArgs
    {
        public IPlugDataContext[] SenderPlugs { get; }

        public StartCreateConnectionEventArgs(IPlugDataContext[] startPlugs)
        {
            SenderPlugs = startPlugs;
        }
    }

    //! コネクションの生成確定時に発行されるイベント
    public class CompletedCreateConnectionEventArgs : EventArgs
    {
        public IConnectionDataContext ConnectionDataContext { get; }

        public CompletedCreateConnectionEventArgs(IConnectionDataContext connectionDataContext )
        {
            ConnectionDataContext = connectionDataContext;
        }
    }

    //! ノード移動開始時に発行されるイベント
    public class CompletedMoveNodeEventArgs : EventArgs
    {
        public IReadOnlyDictionary<INodeDataContext, Point> InitialNodePoints { get; }
        public IReadOnlyDictionary<INodeDataContext, Point> CompletedNodePoints { get; }

        public CompletedMoveNodeEventArgs(IReadOnlyDictionary<INodeDataContext, Point> initial , IReadOnlyDictionary<INodeDataContext,Point> completed)
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

    //! 選択が変更されたときに発行されるイベント
    public class SelectionChangedEventArgs : EventArgs
    {
        public ISelectable[] ChangedItems { get; }

        public SelectionChangedEventArgs(ISelectable[] changed)
        {
            ChangedItems = changed;
        }
    }
}
