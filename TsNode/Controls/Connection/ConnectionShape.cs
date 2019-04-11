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
        #region 座標

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

        #endregion

        #region Plug

        public static readonly DependencyProperty SourcePlugProperty = DependencyProperty.Register(
            nameof(SourcePlug), typeof(IPlugDataContext), typeof(ConnectionShape), new PropertyMetadata(default(IPlugDataContext), OnSourcePlugChanged));

        public IPlugDataContext SourcePlug
        {
            get => (IPlugDataContext) GetValue(SourcePlugProperty);
            set => SetValue(SourcePlugProperty, value);
        }

        public static readonly DependencyProperty DestPlugProperty = DependencyProperty.Register(
            nameof(DestPlug), typeof(IPlugDataContext), typeof(ConnectionShape), new PropertyMetadata(default(IPlugDataContext),OnDestPlugChanged));

        public IPlugDataContext DestPlug
        {
            get => (IPlugDataContext) GetValue(DestPlugProperty);
            set => SetValue(DestPlugProperty, value);
        }

        #endregion

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

        //! SourcePlugが更新された
        public static void OnSourcePlugChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConnectionShape shape)
            {
                // 古いノードの座標イベントへのバインドを解除する
                shape.un_bind_node(shape._sourcePlugControl, shape._associationSourceNode, shape.UpdateSourcePointFromNode);
                shape._sourcePlugControl = shape.find_plug_by_datacontext(e.NewValue);

                // ノードの座標が更新されたらSourcePointを再計算する
                shape.bind_node(shape._sourcePlugControl, shape._associationSourceNode, shape.UpdateSourcePointFromNode);

                shape.setup_plug_start_point();
            }
        }
        
        //! DestPlugが更新された
        private static void OnDestPlugChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConnectionShape shape)
            {
                // 古いノードの座標イベントへのバインドを解除する
                shape.un_bind_node(shape._destPlugControl, shape._associationDestNode,shape.UpdateDestPointFromNode);
                shape._destPlugControl = shape.find_plug_by_datacontext(e.NewValue);

                // ノードの座標が更新されたらDestPointを再計算する
                shape.bind_node(shape._destPlugControl, shape._associationDestNode, shape.UpdateDestPointFromNode);

                shape.setup_plug_start_point();
            }
        }

        //! 片方がのプラグがバインドされもう片方がnullのときにSourceとDestを一時的に統一する
        //  これをしないと数フレーム線が飛ぶので目にうるさい
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

        // プラグのDataContextからPlugControlを検索する 
        private PlugControl find_plug_by_datacontext(object dataContext)
        {
            return this.FindVisualParentWithType<NetworkView>()
                       .FindChildWithDataContext<PlugControl>(dataContext);
        }

        // プラグの親ノードを検索し座標イベントの移動に指定したイベントを張り付ける
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

        // プラグの親ノードを検索し座標イベントの移動から指定したイベントを削除する
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

        // Source座標を更新する
        private void UpdateSourcePointFromNode(object sender , UpdateNodePointArgs e)
        {
            var relativePoint = _sourcePlugControl.GetNodeFromPoint(new Point(6, 6));
            SourceX = relativePoint.X + e.Point.X;
            SourceY = relativePoint.Y + e.Point.Y;
        }

        // Dest座標を更新する
        private void UpdateDestPointFromNode(object sender, UpdateNodePointArgs e)
        {
            var relativePoint = _destPlugControl.GetNodeFromPoint(new Point(6, 6));
            DestX = relativePoint.X + e.Point.X;
            DestY = relativePoint.Y + e.Point.Y;
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                // 開始点 / 終了点から 形状を決める
                var info = ConnectionHelper.CalcBezierInfo(SourceX, SourceY, DestX, DestY);
                return ConnectionHelper.MakeBezierPathGeometry(info);
            }
        }
    }
}
