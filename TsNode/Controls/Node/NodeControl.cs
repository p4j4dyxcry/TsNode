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
    /// <summary>
    /// ノードを扱うコントロール
    /// </summary>
    public class NodeControl : ContentControl , ISelectable
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(NodeControl), new PropertyMetadata(default(bool)));
        
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

        //! 座標が更新されたときに発行されるイベント
        public event Action<object, UpdateNodePointArgs> UpdatePoints;

        // cache
        private PlugItemsControl _inputPlugItemsControl;
        private PlugItemsControl _outputPlugItemsControl;

        private static readonly string PART_InputPlugItemsControl  = nameof(PART_InputPlugItemsControl);
        private static readonly string PART_OutputPlugItemsControl = nameof(PART_OutputPlugItemsControl);

        //! 入力プラグを取得する
        public IEnumerable<PlugControl> GetInputPlugs()
        {
            if (_inputPlugItemsControl is null)
            {
                _inputPlugItemsControl = this.FindChildWithName<PlugItemsControl>(PART_InputPlugItemsControl);
                Debug.Assert(_inputPlugItemsControl != null, $"NodeのDataTemplateに{PART_InputPlugItemsControl}という名前のPlugItemsControlを実装する必要があります。");
            }

            return _inputPlugItemsControl.FindVisualChildrenWithType<PlugControl>().ToArray();
        }

        //! 出力プラグを取得する
        public IEnumerable<PlugControl> GetOutputPlugs()
        {
            if (_outputPlugItemsControl is null)
            {
                _outputPlugItemsControl = this.FindChildWithName<PlugItemsControl>(PART_OutputPlugItemsControl);
                Debug.Assert(_outputPlugItemsControl != null, $"NodeのDataTemplateに{PART_OutputPlugItemsControl}という名前のPlugItemsControlを実装する必要があります。");
            }
            return _outputPlugItemsControl.FindVisualChildrenWithType<PlugControl>().ToArray();
        }
    }
}
