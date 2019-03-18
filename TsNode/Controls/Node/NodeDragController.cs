using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TsNode.Interface;

namespace TsNode.Controls.Node
{
    public class NodeDragController : IDragController
    {
        private readonly Point _dragStartPos;
        private readonly IReadOnlyDictionary<NodeControl, Point> _originalPoints;
        private readonly bool _useSnapGrid;
        private readonly int _gridSize;
        private readonly NodeControl[] _selectedNodes;
        private readonly IInputElement _inputElement;
        private readonly ICommand _completedNodeMove;

        private bool _isDrag;
        private bool _isMoved;

        public NodeDragController(MouseEventArgs args, NodeControl[] nodes, NodeControl[] selectedNodes, IInputElement sender, int gridSize, bool useSnapGrid, ICommand completedNodeMov)
        {
            if (args.LeftButton == MouseButtonState.Pressed)
            {
                _isDrag = true;

                _dragStartPos = args.GetPosition(sender);
                _originalPoints = nodes.ToDictionary(x => x , x => new Point(x.X, x.Y));
            }

            _completedNodeMove = completedNodeMov;
            _selectedNodes = selectedNodes;
            _useSnapGrid = useSnapGrid;
            _inputElement = sender;
            _gridSize = gridSize;
        }


        public bool CanDragStart(object sender, MouseEventArgs args)
        {
            return _selectedNodes.Any() && _isDrag;
        }

        public void OnDrag(object sender, MouseEventArgs args)
        {
            if (_isDrag is false)
                return;
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
                var x = (int)(_originalPoints[item].X + xDelta);
                var y = (int)(_originalPoints[item].Y + yDelta);

                var point = _useSnapGrid ? gird_snap(x, y, 4) : new Point(x, y);

                item.X = point.X;
                item.Y = point.Y;
            }
        }

        public void DragEnd(object sender, MouseEventArgs args)
        {
            Cancel();
        }

        public void Cancel()
        {
            if (_isDrag)
            {
                _isDrag = false;
                if (_isMoved)
                {
                    var initial = _originalPoints
                        .Where(x => x.Key.IsSelected is true)
                        .ToDictionary(x => x.Key.DataContext as INodeViewModel, x => x.Value);

                    var completed = _selectedNodes.ToDictionary(x => x.DataContext as INodeViewModel, x => new Point(x.X, x.Y));

                    _completedNodeMove?.Execute(new CompletedMoveNodeEventArgs(initial, completed));
                }
            }
        }

        private Point gird_snap(int x, int y, int subGrid )
        {
            var gridDelta = Math.Max(_gridSize / subGrid, subGrid);

            x = x + gridDelta / subGrid;
            y = y + gridDelta / subGrid;

            return new Point(gridDelta * (x / gridDelta), gridDelta * (y / gridDelta));
        }
    }
}
