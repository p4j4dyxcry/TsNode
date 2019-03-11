using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TsNode.Controls.Plug;
using TsNode.Interface;

namespace TsNode.Controls.Node
{
    public class NodeControl : ContentControl , ISelectable
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(NodeControl), new PropertyMetadata(default(bool), OnIsSelectedChanged));


        public bool IsSelected
        {
            get => (bool) GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty XProperty = DependencyProperty.Register(
            nameof(X), typeof(double), typeof(NodeControl), new PropertyMetadata(default(double), OnPointsChanged));

        public double X
        {
            get => (double) GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
            nameof(Y), typeof(double), typeof(NodeControl), new PropertyMetadata(default(double), OnPointsChanged));

        public double Y
        {
            get => (double) GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        private NodeItemsControl _parent;

        public static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NodeControl nodeControl)
            {
                if (e.OldValue != e.NewValue)
                {
                    nodeControl.UpdatePoints?.Invoke(nodeControl,  new UpdateNodePointArgs(nodeControl.X,nodeControl.Y));
                }
            }
        }

        public static void OnIsSelectedChanged(DependencyObject d , DependencyPropertyChangedEventArgs e)
        {
            if (d is NodeControl nodeControl)
            {
                if (e.OldValue != e.NewValue)
                {
                    if (nodeControl._parent is null)
                        nodeControl._parent = nodeControl.FindVisualParentWithType<NodeItemsControl>();
                    nodeControl._parent.RaiseSelectionChanged(nodeControl, new EventArgs());
                }
            }
        }

        public event Action<object, UpdateNodePointArgs> UpdatePoints;

        private PlugItemsControl _inputPlugItemsControl;
        private PlugItemsControl _outputPlugItemsControl;
        public IEnumerable<PlugControl> GetInputPlugs()
        {
            if (_inputPlugItemsControl is null)
                _inputPlugItemsControl = this.FindChildWithName<PlugItemsControl>("PART_InputPlugItemsControl");
            Debug.Assert(_inputPlugItemsControl != null);

            return _inputPlugItemsControl.FindVisualChildrenWithType<PlugControl>().ToArray();
        }

        public IEnumerable<PlugControl> GetOutputPlugs()
        {
            if (_outputPlugItemsControl is null)
                _outputPlugItemsControl = this.FindChildWithName<PlugItemsControl>("PART_OutputPlugItemsControl");
            Debug.Assert(_outputPlugItemsControl != null);

            return _outputPlugItemsControl.FindVisualChildrenWithType<PlugControl>().ToArray();
        }
    }
}
