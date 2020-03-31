using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TsNode.Foundations;
using TsNode.Interface;

namespace TsNode.Controls.Drag.Controller
{
    /// <summary>
    /// NodeDragControllerのセットアップデータ
    /// </summary>
    public class NodeDragControllerSetupArgs
    {
        //! ノードの相対座標を計算するためのコントロール
        public IInputElement BaseControl { get; }

        //! ドラッグ対象ノード
        public INodeControl[]  Nodes { get; }

        //! スナップグリッド利用フラグ
        public bool UseSnapGrid { get; set; } = true;

        //! スナップグリッド幅
        public int GridSize { get; set; } = 5;

        //! ノードのドラッグ完了コマンド
        //! コマンド引数として CompletedCreateConnectionEventArgs が渡される
        public ICommand CompletedCommand { get; }
        
        public InfiniteScrollViewer InfiniteScrollViewer { get; }

        //! 最低限の引数はコンストラクタでセットアップする必要がある
        public NodeDragControllerSetupArgs(IInputElement baseControl,
                                    INodeControl[] nodes,
                                    ICommand completedCommand = null , 
                                    InfiniteScrollViewer scrollViewer = null)
        {
            BaseControl = baseControl;
            Nodes = nodes;
            CompletedCommand = completedCommand;
            InfiniteScrollViewer = scrollViewer;
        }
    }

    /// <summary>
    /// ノードのドラッグを行うコントローラ
    /// </summary>
    public class NodesDragController : IDragController
    {
        // ドラッグする前のノードの座標を覚える
        private readonly IReadOnlyDictionary<INodeControl, Point> _originalPoints;

        // ノードの移動処理にスナップグリッド使用するかどうか
        private readonly bool _useSnapGrid;

        // スナップ単位(正方形)
        private readonly int _gridSize;

        // 移動を行うノード一覧
        private readonly INodeControl[] _selectedNodes;

        // ノードの座標計算を行うためのコントロール(Canvas等が指定可能)
        private readonly IInputElement _inputElement;

        // ノードの移動完了コマンド　引数として CompletedCreateConnectionEventArgs が渡される
        private readonly ICommand _completedNodeMove;

        private bool _isDrag;
        private bool _isMoved;
        private bool _isCaptured;

        private readonly InfiniteScrollViewer _scrollViewer;

        // コンストラクタ / ドラッグ開始
        public NodesDragController(NodeDragControllerSetupArgs setupArgs)
        {
            _originalPoints = setupArgs.Nodes.ToDictionary(x => x , x => new Point(x.X, x.Y));
            
            _completedNodeMove = setupArgs.CompletedCommand;
            _selectedNodes = setupArgs.Nodes.Where(x=>x.CanMovable).ToArray();
            _useSnapGrid = setupArgs.UseSnapGrid;
            _gridSize = setupArgs.GridSize;
            _inputElement = setupArgs.BaseControl;
            _scrollViewer = setupArgs.InfiniteScrollViewer;
        }

        public bool CanDragStart(DragControllerEventArgs args)
        {
            return _selectedNodes.Any() && args.Button == MouseButton.Left;
        }

        public void OnStartDrag(DragControllerEventArgs args)
        {
            _isDrag = true;
        }

        // ドラッグ中の処理
        public void OnDragMoving(DragControllerEventArgs args)
        {
            if (_isDrag is false)
                return;
            
            if (_inputElement != null && Mouse.Captured == null && _isCaptured is false)
            {
                _isCaptured = _inputElement.CaptureMouse();
            }

            // ボタンが離れた場合はキャンセルとみなす
            if (args.Button != MouseButton.Left)
            {
                Cancel();
                return;
            }
            _isMoved = true;

            foreach (var item in _selectedNodes)
            {
                // ドラッグ開始前 + 移動量でノード位置を計算する
                var x = (int)(_originalPoints[item].X + args.Vector.X);
                var y = (int)(_originalPoints[item].Y + args.Vector.Y);

                //! スナップグリッドする場合は再計算
                var point = _useSnapGrid ? gird_snap(x, y, 4) : new Point(x, y);

                item.X = point.X;
                item.Y = point.Y;
            }

            if (IsMouseOutScrollViewer())
            {
                DragScrollOffset();
                _scrollViewer.UpdateScrollBar();
                return;
            }
        }

        public bool IsMouseOutScrollViewer()
        {
            if (_scrollViewer is null)
                return false;
            
            var pos = Mouse.GetPosition(_scrollViewer);
            if (pos.X > 0 &&
                pos.X < _scrollViewer.ActualWidth &&
                pos.Y > 0 &&
                pos.Y < _scrollViewer.ActualHeight)
                return false;

            return true;
        }
        
        private void DragScrollOffset()
        {
            var offset = new Point(0, 0);
            var mousePoint = Mouse.GetPosition(_scrollViewer);
 
            if (mousePoint.X < 0)
            {
                offset.X = - mousePoint.X;
            }
            else if(mousePoint.X > _scrollViewer.ActualWidth)
            {
                offset.X = -(mousePoint.X - _scrollViewer.ActualWidth);
            }
            
            if (mousePoint.Y < 0)
            {
                offset.Y = - mousePoint.Y;
            }
            else if (mousePoint.Y > _scrollViewer.ActualHeight)
            {
                offset.Y = -(mousePoint.Y - _scrollViewer.ActualHeight);
            }

            // clamp
            var clampValue = _scrollViewer.ScrollOffsetClampValue;
            offset.X = Math.Max(Math.Min(clampValue, offset.X), -clampValue);
            offset.Y = Math.Max(Math.Min(clampValue, offset.Y), -clampValue);
            
            _scrollViewer.TranslateX(offset.X *  _scrollViewer.ScrollRate);
            _scrollViewer.TranslateY(offset.Y *  _scrollViewer.ScrollRate);
 }
        
        public void OnDragEnd(DragControllerEventArgs args)
        {
            Completed();
        }

        // ドラッグ完了処理
        private void Completed()
        {
            if(_isCaptured)
                _inputElement.ReleaseMouseCapture();
            
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
