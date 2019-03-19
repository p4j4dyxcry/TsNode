using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TsNode.Interface;

namespace TsNode.Controls.Node
{
    /// <summary>
    /// NodeDragControllerのセットアップデータ
    /// </summary>
    public class NodeDragControllerSetupArgs
    {
        //! ノードの相対座標を計算するためのコントロール
        public IInputElement BaseControl { get; }

        //! マウスイベント
        public MouseEventArgs Args  { get; }

        //! ドラッグ対象ノード
        public NodeControl[]  Nodes { get; }

        //! スナップグリッド利用フラグ
        public bool UseSnapGrid { get; set; } = true;

        //! スナップグリッド幅
        public int GridSize { get; set; } = 5;

        //! ノードのドラッグ完了コマンド
        //! コマンド引数として CompletedCreateConnectionEventArgs が渡される
        public ICommand CompletedCommand { get; }

        //! 最低限の引数はコンストラクタでセットアップする必要がある
        public NodeDragControllerSetupArgs(IInputElement basecontrol,
                                    MouseEventArgs mouseEventArgs,
                                    NodeControl[] nodes,
                                    ICommand completedCommand = null)
        {
            BaseControl = basecontrol;
            Args = mouseEventArgs;
            Nodes = nodes;
            CompletedCommand = completedCommand;
        }
    }

    /// <summary>
    /// ノードのドラッグを行うコントローラ
    /// </summary>
    public class NodeDragController : IDragController
    {
        // ドラッグ開始位置
        private readonly Point _dragStartPos;

        // ドラッグする前のノードの座標を覚える
        private readonly IReadOnlyDictionary<NodeControl, Point> _originalPoints;

        // ノードの移動処理にスナップグリッド使用するかどうか
        private readonly bool _useSnapGrid;

        // スナップ単位(正方形)
        private readonly int _gridSize;

        // 移動を行うノード一覧
        private readonly NodeControl[] _selectedNodes;

        // ノードの座標計算を行うためのコントロール(Canvas等が指定可能)
        private readonly IInputElement _inputElement;

        // ノードの移動完了コマンド　引数として CompletedCreateConnectionEventArgs が渡される
        private readonly ICommand _completedNodeMove;

        private bool _isDrag;
        private bool _isMoved;

        // コンストラクタ / ドラッグ開始
        public NodeDragController(NodeDragControllerSetupArgs setupArgs)
        {
            if (setupArgs.Args.LeftButton == MouseButtonState.Pressed)
            {
                _isDrag = true;

                _dragStartPos = setupArgs.Args.GetPosition(setupArgs.BaseControl);
                _originalPoints = setupArgs.Nodes.ToDictionary(x => x , x => new Point(x.X, x.Y));
            }

            _completedNodeMove = setupArgs.CompletedCommand;
            _selectedNodes = setupArgs.Nodes;
            _useSnapGrid = setupArgs.UseSnapGrid;
            _inputElement = setupArgs.BaseControl;
            _gridSize = setupArgs.GridSize;
        }

        public bool CanDragStart(object sender, MouseEventArgs args)
        {
            return _selectedNodes.Any() && _isDrag;
        }

        // ドラッグ中の処理
        public void OnDrag(object sender, MouseEventArgs args)
        {
            if (_isDrag is false)
                return;

            // ボタンが離れた場合はキャンセルとみなす
            if (args.LeftButton != MouseButtonState.Pressed)
            {
                Cancel();
                return;
            }

            var currentPos = args.GetPosition(_inputElement);
            var xDelta = currentPos.X - _dragStartPos.X;
            var yDelta = currentPos.Y - _dragStartPos.Y;

            _isMoved = true;

            foreach (var item in _selectedNodes)
            {
                // ドラッグ開始前 + 移動量でノード位置を計算する
                var x = (int)(_originalPoints[item].X + xDelta);
                var y = (int)(_originalPoints[item].Y + yDelta);

                //! スナップグリッドする場合は再計算
                var point = _useSnapGrid ? gird_snap(x, y, 4) : new Point(x, y);

                item.X = point.X;
                item.Y = point.Y;
            }
        }

        public void DragEnd(object sender, MouseEventArgs args)
        {
            Completed();
        }

        // ドラッグ完了処理
        private void Completed()
        {
            if (_isDrag)
            {
                _isDrag = false;
                if (_isMoved & _completedNodeMove != null)
                {
                    // ドラッグ開始前座標
                    var initial = _originalPoints.ToDictionary(x => x.Key.DataContext as INodeDataContext, x => x.Value);

                    // ドラッグ完了後座標
                    var completed = _selectedNodes.ToDictionary(x => x.DataContext as INodeDataContext, x => new Point(x.X, x.Y));

                    //! 完了コマンドを発行する(undo / redo したい場合等に利用する)
                    _completedNodeMove?.Execute(new CompletedMoveNodeEventArgs(initial, completed));
                }
            }
        }

        public void Cancel()
        {
            Completed();
        }

        // スナップグリッド( 座標丸め )
        private Point gird_snap(int x, int y, int subGrid )
        {
            var gridDelta = Math.Max(_gridSize / subGrid, subGrid);

            x = x + gridDelta / subGrid;
            y = y + gridDelta / subGrid;

            return new Point(gridDelta * (x / gridDelta), gridDelta * (y / gridDelta));
        }
    }
}
