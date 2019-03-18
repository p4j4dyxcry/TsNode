using System.Windows;
using System.Windows.Controls;

namespace TsProperty
{
    /// <summary>
    /// ラベル付きコントロール
    /// </summary>
    public class LabeledControl : ContentControl
    {
        /// <summary>
        /// ラベル幅を指定します
        /// </summary>
        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(
            nameof(LabelWidth), typeof(double), typeof(LabeledControl), new PropertyMetadata(double.NaN));

        public double LabelWidth
        {
            get => (double) GetValue(LabelWidthProperty);
            set => SetValue(LabelWidthProperty, value);
        }

        /// <summary>
        /// ラベルに表示するメッセージを指定します
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(LabeledControl), new PropertyMetadata(string.Empty));

        public string Label
        {
            get => (string) GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        static LabeledControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LabeledControl),
                new FrameworkPropertyMetadata(typeof(LabeledControl)));
        }
    }
}
