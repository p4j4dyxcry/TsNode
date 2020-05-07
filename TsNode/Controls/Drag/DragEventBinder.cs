using System;
using System.Windows;
using System.Windows.Input;
using TsNode.Interface;

namespace TsNode.Controls.Drag
{
    public class DragEventBinder : IDisposable
    {
        // events
        public EventHandler<DragControllerEventArgs> PreviewDragStart;
        
        // readonly properties
        private readonly UIElement _target;
        private readonly Func<DragControllerEventArgs, IDragController> _converter;
        private readonly bool _onClickFocus;
        private readonly DragEventBinder _parent;

        // properties
        private IDragController _currentController;
        private bool _mouseCaptured = false;
        private Point _startPoint;
        private Point _prevPoint;
        
        public DragEventBinder(UIElement target , Func<DragControllerEventArgs , IDragController> converter , bool onClickFocus , DragEventBinder parent = null)
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
            
            _startPoint = args.GetPosition(_target);
            _prevPoint = _startPoint;

            var convertedArgs = convert_args(args);
            
            PreviewDragStart?.Invoke(this,convertedArgs);

            _currentController = _converter(convertedArgs);

            if (_currentController == null)
            {
                return;
            }

            if (_currentController.CanDragStart(convertedArgs) is false)
            {
                try_cancel();
                return;
            }

            if(_onClickFocus)
                _target.Focus();

            _currentController.OnStartDrag(convertedArgs);
            
            args.Handled = true;
        }
        
        private void on_drag(object sender , MouseEventArgs args)
        {
            if (check_parent_event_doing())
                return;
            
            if (_currentController is null)
                return;

            // マウスクリックしていないのでキャンセル
            if (IsMousePressed(args) is false)
            {
                try_cancel();
                return;
            }

            var convertArgs = convert_args(args);
            
            _currentController.OnDragMoving(convertArgs);

            _prevPoint = convertArgs.CurrentPoint;

            args.Handled = true;
        }
        
        private void on_mouse_up(object sender , MouseEventArgs args)
        {
            if (check_parent_event_doing())
                return;
            
            if (_currentController is null)
                return;
            
            _currentController.OnDragEnd(convert_args(args));

            if (try_cancel())
            {
               args.Handled = true;
            }
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

        private bool IsMousePressed( MouseEventArgs args)
        {
            return args.LeftButton   == MouseButtonState.Pressed || 
                   args.RightButton  == MouseButtonState.Pressed ||
                   args.MiddleButton == MouseButtonState.Pressed;
        }

        private bool try_cancel()
        {
            if (_currentController != null)
            {
                _currentController.Cancel();
                _currentController = null;
                return true;
            }

            return false;
        }

        public void on_preview_mouse_up(object sender, MouseEventArgs args)
        {
            try_release_capture();
        }
        
        public void on_preview_mouse_move(object sender, MouseEventArgs args)
        {
            try_capture();
        }

        public void try_capture()
        {
            if (_currentController is IUseMouseCaptureTarget captureTarget)
            {
                if (_mouseCaptured is false)
                {
                    _mouseCaptured = true;
                    captureTarget?.Capture();
                }
            }
        }

        public void try_release_capture()
        {
            if (_currentController is IUseMouseCaptureTarget captureTarget)
            {
                if (_mouseCaptured is true)
                {
                    captureTarget?.ReleaseCapture();
                    _mouseCaptured = false;
                }
            }
        }

        private void bind_events()
        {
            _target.MouseDown += on_mouse_down;
            _target.MouseMove += on_drag;
            _target.PreviewMouseMove += on_preview_mouse_move;
            _target.PreviewMouseUp += on_preview_mouse_up;
            _target.MouseUp += on_mouse_up;
        }

        private void un_bind_events()
        {
            _target.MouseDown -= on_mouse_down;
            _target.MouseMove -= on_drag;
            _target.PreviewMouseMove -= on_preview_mouse_move;
            _target.PreviewMouseUp -= on_preview_mouse_up;
            _target.MouseUp += on_mouse_up;
        }

        private bool check_parent_event_doing()
        {
            return _parent?._currentController != null;
        }
    }
}