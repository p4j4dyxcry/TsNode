﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TsNode.Controls.Drag.Controller;
using TsNode.Interface;

namespace TsNode.Controls.Drag
{
    public class RectSelectionDragBuild : IDragControllerBuild
    {
        public int Priority { get; }

        private readonly DragControllerBuilder _dragControllerBuilder;
        private Style SelectionRectangleStyle { get; }
        private readonly Panel _panel;

        public RectSelectionDragBuild(DragControllerBuilder dragControllerBuilder, int priority, Style selectionRectangleStyle , Panel panel)
        {
            _dragControllerBuilder = dragControllerBuilder;
            Priority = priority;
            SelectionRectangleStyle = selectionRectangleStyle;
            _panel = panel;
        }

        public bool TryBuild()
        {
            return _dragControllerBuilder.MouseButton == MouseButton.Left && _dragControllerBuilder.ModifierNone();
        }

        public IDragController Build()
        {
            var setupArgs = new RectSelectionControllerSetupArgs(
                _dragControllerBuilder.Nodes, 
                _dragControllerBuilder.ConnectionShapes,
                _panel)
            {
                SelectionChangedCommand = _dragControllerBuilder.SelectionChangedCommand,
                RectangleStyle = SelectionRectangleStyle
            };
            return new RectSelectionController(setupArgs);
        }
    }
}
