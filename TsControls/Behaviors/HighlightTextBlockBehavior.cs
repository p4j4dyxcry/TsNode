using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace TsControls.Behaviors
{
    public class HighlightTextBlockBehavior : Behavior<TextBlock>
    {
        public static readonly DependencyProperty HighlightTextTextProperty = DependencyProperty.Register(
            nameof(HighlightedText), typeof(string), typeof(HighlightTextBlockBehavior), new PropertyMetadata(default(string), HighlightedTextChanged));

        private static void HighlightedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is HighlightTextBlockBehavior behavior)
                behavior.OnHighlight();
        }

        public string HighlightedText
        {
            get => (string) GetValue(HighlightTextTextProperty);
            set => SetValue(HighlightTextTextProperty, value);
        }

        public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.Register(
            nameof(HighlightBrush), typeof(Brush), typeof(HighlightTextBlockBehavior), new PropertyMetadata(Brushes.DodgerBlue));

        public Brush HighlightBrush
        {
            get => (Brush) GetValue(HighlightBrushProperty);
            set => SetValue(HighlightBrushProperty, value);
        }

        public void OnHighlight()
        {
            if (AssociatedObject.Text is null || AssociatedObject.Text == string.Empty)
                return;

            if (HighlightedText is null || HighlightedText == string.Empty)
                return;

            var targetText = HighlightedText.ToLower();
            var lowerText = AssociatedObject.Text.ToLower();
            var originalText = AssociatedObject.Text;
            if (!string.IsNullOrEmpty(HighlightedText) && lowerText.Contains(targetText))
            {
                AssociatedObject.Inlines.Clear();
                var point = lowerText.IndexOf(targetText, StringComparison.Ordinal);
                var strHighlighted = originalText.Substring(point, HighlightedText.Length);
                var runHighlight = new Run(strHighlighted) { Background = HighlightBrush };
                if (point == 0)
                {
                    AssociatedObject.Inlines.Add(runHighlight);
                    var remainingLength = lowerText.Length - (point + targetText.Length);
                    var text = originalText.Substring((point + targetText.Length), remainingLength);
                    AssociatedObject.Inlines.Add(new Run(text));
                }
                else
                {
                    var firstPart = originalText.Substring(0, point);
                    AssociatedObject.Inlines.Add(new Run(firstPart));
                    AssociatedObject.Inlines.Add(runHighlight);
                    var remainingLength = lowerText.Length - (point + targetText.Length);
                    var text = originalText.Substring((point + targetText.Length), remainingLength);
                    AssociatedObject.Inlines.Add(new Run(text));
                }
            }
            else
            {
                AssociatedObject.Inlines.Clear();
                AssociatedObject.Inlines.Add(new Run(originalText));
            }
        }
    }
}