using System.Windows;
using System.Windows.Controls;

namespace TsControls.Controls
{
    /// <summary>
    /// ラベル付きコントロール
    /// </summary>
    public class LabeledControl : ContentControl
    {
        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(
            nameof(LabelWidth), typeof(GridLength), typeof(LabeledControl), new PropertyMetadata(new GridLength(25, GridUnitType.Star)));

        public GridLength LabelWidth
        {
            get => (GridLength)GetValue(LabelWidthProperty);
            set => SetValue(LabelWidthProperty, value);
        }

        public static readonly DependencyProperty ContentWidthProperty = DependencyProperty.Register(
            nameof(ContentWidth), typeof(GridLength), typeof(LabeledControl), new PropertyMetadata(new GridLength(75, GridUnitType.Star)));

        public GridLength ContentWidth
        {
            get => (GridLength)GetValue(ContentWidthProperty);
            set => SetValue(ContentWidthProperty, value);
        }

        /// <summary>
        /// ラベルに表示するメッセージを指定します
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(LabeledControl), new PropertyMetadata(string.Empty));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty LabelVerticalAlligmentProperty = DependencyProperty.Register(
            nameof(LabelVerticalAlligment), typeof(VerticalAlignment), typeof(LabeledControl), new PropertyMetadata(VerticalAlignment.Center));

        public string LabelVerticalAlligment
        {
            get => (string)GetValue(LabelVerticalAlligmentProperty);
            set => SetValue(LabelVerticalAlligmentProperty, value);
        }

        public static readonly DependencyProperty LabelHorizontalAlligmentProperty = DependencyProperty.Register(
            nameof(LabelHorizontalAlligment), typeof(HorizontalAlignment), typeof(LabeledControl), new PropertyMetadata(HorizontalAlignment.Stretch));

        public string LabelHorizontalAlligment
        {
            get => (string)GetValue(LabelHorizontalAlligmentProperty);
            set => SetValue(LabelHorizontalAlligmentProperty, value);
        }

        static LabeledControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LabeledControl),
                new FrameworkPropertyMetadata(typeof(LabeledControl)));
        }
    }
}
