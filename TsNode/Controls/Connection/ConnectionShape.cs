using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using TsNode.Controls.Node;
using TsNode.Controls.Plug;
using TsNode.Interface;

namespace TsNode.Controls.Connection
{
    public class ConnectionShape : Shape, ISelectable
    {
        public static readonly DependencyProperty SourceXProperty = DependencyProperty.Register(
            nameof(SourceX), typeof(double), typeof(ConnectionShape), new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender));

        public double SourceX
        {
            get => (double)GetValue(SourceXProperty);
            set => SetValue(SourceXProperty, value);
        }

        public static readonly DependencyProperty SourceYProperty = DependencyProperty.Register(
            nameof(SourceY), typeof(double), typeof(ConnectionShape), new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender));

        public double SourceY
        {
            get => (double)GetValue(SourceYProperty);
            set => SetValue(SourceYProperty, value);
        }

        public static readonly DependencyProperty DestXProperty = DependencyProperty.Register(
            nameof(DestX), typeof(double), typeof(ConnectionShape), new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender));

        public double DestX
        {
            get => (double)GetValue(DestXProperty);
            set => SetValue(DestXProperty, value);
        }

        public static readonly DependencyProperty DestYProperty = DependencyProperty.Register(
            nameof(DestY), typeof(double), typeof(ConnectionShape), new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender));

        public double DestY
        {
            get => (double)GetValue(DestYProperty);
            set => SetValue(DestYProperty, value);
        }

        public static readonly DependencyProperty SourcePlugProperty = DependencyProperty.Register(
            nameof(SourcePlug), typeof(IPlugViewModel), typeof(ConnectionShape), new PropertyMetadata(default(IPlugViewModel), OnSourcePlugChanged));

        public IPlugViewModel SourcePlug
        {
            get => (IPlugViewModel) GetValue(SourcePlugProperty);
            set => SetValue(SourcePlugProperty, value);
        }

        public static readonly DependencyProperty DestPlugProperty = DependencyProperty.Register(
            nameof(DestPlug), typeof(IPlugViewModel), typeof(ConnectionShape), new PropertyMetadata(default(IPlugViewModel),OnDestPlugChanged));

        public IPlugViewModel DestPlug
        {
            get => (IPlugViewModel) GetValue(DestPlugProperty);
            set => SetValue(DestPlugProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(ConnectionShape));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        private readonly HashSet<NodeControl> _associationDestNode = new HashSet<NodeControl>();
        private readonly HashSet<NodeControl> _associationSourceNode = new HashSet<NodeControl>();

        private PlugControl _sourcePlugControl;
        private PlugControl _destPlugControl;

        public static void OnSourcePlugChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConnectionShape shape)
            {
                shape.un_bind_node(shape._sourcePlugControl, shape._associationSourceNode, shape.UpdateSourcePointFromNode);
                shape._sourcePlugControl = shape.find_plug_by_datacontext(e.NewValue);

                shape.bind_node(shape._sourcePlugControl, shape._associationSourceNode, shape.UpdateSourcePointFromNode);

                shape.setup_plug_start_point();
            }
        }
        private static void OnDestPlugChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConnectionShape shape)
            {
                shape.un_bind_node(shape._destPlugControl, shape._associationDestNode,shape.UpdateDestPointFromNode);
                shape._destPlugControl = shape.find_plug_by_datacontext(e.NewValue);

                shape.bind_node(shape._destPlugControl, shape._associationDestNode, shape.UpdateDestPointFromNode);

                shape.setup_plug_start_point();
            }
        }

        private void setup_plug_start_point()
        {
            if (_sourcePlugControl is null)
            {
                SourceX = DestX;
                SourceY = DestY;
            }
            else if (_destPlugControl is null)
            {
                DestX = SourceX;
                DestY = SourceY;
            }
        }

        private PlugControl find_plug_by_datacontext(object dataContext)
        {
            return this.FindVisualParentWithType<NetworkView>()
                       .FindChildWithDataContext<PlugControl>(dataContext);
        }

        private void bind_node(PlugControl plug, HashSet<NodeControl> hashSet, Action<object, UpdateNodePointArgs> bindFunc)
        {
            if (plug is null)
                return;

            if (hashSet.Contains(plug.ParentNode) is false)
            {
                plug.ParentNode.UpdatePoints += bindFunc;
                hashSet.Add(plug.ParentNode);
            }
            bindFunc?.Invoke(plug.ParentNode, new UpdateNodePointArgs(plug.ParentNode.X, plug.ParentNode.Y));
        }

        private void un_bind_node(PlugControl plug, HashSet<NodeControl> hashSet ,Action<object,UpdateNodePointArgs> bindFunc)
        {
            if (plug is null)
                return;

            if (hashSet.Contains(plug.ParentNode) is true)
            {
                plug.ParentNode.UpdatePoints -= bindFunc;
                hashSet.Remove(plug.ParentNode);
            }
        }

        private void un_bind_all()
        {
            foreach (var nodeControl in _associationSourceNode)
                nodeControl.UpdatePoints -= UpdateSourcePointFromNode;

            foreach (var nodeControl in _associationDestNode)
                nodeControl.UpdatePoints -= UpdateDestPointFromNode;

            return;
        }

        private void UpdateSourcePointFromNode(object sender , UpdateNodePointArgs e)
        {
            var relativePoint = _sourcePlugControl.GetNodeFromPoint(new Point(6, 6));
            SourceX = relativePoint.X + e.Point.X;
            SourceY = relativePoint.Y + e.Point.Y;
        }

        private void UpdateDestPointFromNode(object sender, UpdateNodePointArgs e)
        {
            var relativePoint = _destPlugControl.GetNodeFromPoint(new Point(6, 6));
            DestX = relativePoint.X + e.Point.X;
            DestY = relativePoint.Y + e.Point.Y;
        }

        public ConnectionShape()
        {
            Unloaded += (s, e) => { un_bind_all(); };
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                var info = ConnectionHelper.CalcBezierInfo(SourceX, SourceY, DestX, DestY);
                return ConnectionHelper.MakeBezierPathGeometry(info);
            }
        }
    }
}
