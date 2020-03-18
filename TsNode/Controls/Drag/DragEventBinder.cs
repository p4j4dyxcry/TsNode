﻿using System;
using System.Windows;
using System.Windows.Input;
using TsNode.Interface;

namespace TsNode.Controls.Drag
{
    public class DragEventBinder : IDisposable
    {
        private readonly UIElement _target;
        private readonly Func<MouseEventArgs, IDragController> _converter;

        private IDragController _currentController;
        private Point _startPoint;
        private Point _prevPoint;
        private readonly bool _onClickFocus;
        private DragEventBinder _parent;
        
        public DragEventBinder(UIElement target , Func<MouseEventArgs , IDragController> converter , bool onClickFocus , DragEventBinder parent = null)
        {
            _parent = parent;
            _target = target;
            _converter = converter;
            _onClickFocus = onClickFocus;

            bind_events();
        }
        
        public void Dispose()
        {
            un_bind_events();
        }
        
        private void on_mouse_down(object sender , MouseEventArgs args)
        {
            if (check_parent_event_doing())
                return;
            
            try_cancel();

            _currentController = _converter(args);

            if (_currentController == null)
            {
                return;
            }

            _startPoint = args.GetPosition(_target);
            _prevPoint = _startPoint;

            var convertedArgs = convert_args(args);
            if (_currentController.CanDragStart(convertedArgs) is false)
            {
                try_cancel();
                return;
            }

            if(_onClickFocus)
                _target.Focus();

            args.Handled = true;
        }
        
        private void on_drag(object sender , MouseEventArgs args)
        {
            if (check_parent_event_doing())
                return;
            
            if (_currentController is null)
                return;

            var convertArgs = convert_args(args);
            
            _currentController.OnDrag(convertArgs);

            _prevPoint = convertArgs.CurrentPoint;

            args.Handled = true;
        }
        
        private void on_mouse_up(object sender , MouseEventArgs args)
        {
            if (check_parent_event_doing())
                return;
            
            if (_currentController is null)
                return;
            
            try_cancel();

            args.Handled = true;
        }

        private DragControllerEventArgs convert_args(MouseEventArgs args)
        {
            var current = args.GetPosition(_target);

            MouseButton button = default;
            if (args.LeftButton == MouseButtonState.Pressed)
                button = MouseButton.Left;
            else if (args.RightButton == MouseButtonState.Pressed)
                button = MouseButton.Right;
            else if (args.MiddleButton == MouseButtonState.Pressed)
                button = MouseButton.Middle;
            
            return new DragControllerEventArgs(_startPoint , current , current - _prevPoint, button);
        }

        private void try_cancel()
        {
            if (_currentController != null)
            {
                _currentController.Cancel();
                _currentController = null;
            }
        }

        private void bind_events()
        {
            _target.MouseDown += on_mouse_down;
            _target.MouseMove += on_drag;
            _target.MouseUp += on_mouse_up;
        }

        private void un_bind_events()
        {
            _target.MouseDown -= on_mouse_down;
            _target.MouseMove -= on_drag;
            _target.MouseUp += on_mouse_up;
        }

        private bool check_parent_event_doing()
        {
            return _parent?._currentController != null;
        }
    }
}