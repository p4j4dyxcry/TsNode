using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace TsControls.Behaviors
{
    public class TextBoxIntellisenseBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable<string>), typeof(TextBoxIntellisenseBehavior), new PropertyMetadata(default(IEnumerable<string>)));

        public IEnumerable<string> ItemsSource
        {
            get => (IEnumerable<string>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty IntellisenseMaxHeightProperty = DependencyProperty.Register(
            nameof(IntellisenseMaxHeight), typeof(double), typeof(TextBoxIntellisenseBehavior), new PropertyMetadata(360d, IntellisenseSizeChanged));

        public double IntellisenseMaxHeight
        {
            get => (double)GetValue(IntellisenseMaxHeightProperty);
            set => SetValue(IntellisenseMaxHeightProperty, value);
        }

        public static readonly DependencyProperty IntellisenseMinHeightProperty = DependencyProperty.Register(
            nameof(IntellisenseMinHeight), typeof(double), typeof(TextBoxIntellisenseBehavior), new PropertyMetadata(120d, IntellisenseSizeChanged));

        public double IntellisenseMinHeight
        {
            get => (double)GetValue(IntellisenseMinHeightProperty);
            set => SetValue(IntellisenseMinHeightProperty, value);
        }

        public static readonly DependencyProperty IntellisenseMinWidthProperty = DependencyProperty.Register(
            nameof(IntellisenseMinWidth), typeof(double), typeof(TextBoxIntellisenseBehavior), new PropertyMetadata(120d, IntellisenseSizeChanged));

        private static void IntellisenseSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBoxIntellisenseBehavior textBoxIntellisenseBehavior)
            {
                textBoxIntellisenseBehavior._listBox.MaxHeight = textBoxIntellisenseBehavior.IntellisenseMaxHeight;
                textBoxIntellisenseBehavior._listBox.MinHeight = textBoxIntellisenseBehavior.IntellisenseMinHeight;
                textBoxIntellisenseBehavior._listBox.MinWidth = textBoxIntellisenseBehavior.IntellisenseMinWidth;
            }
        }

        public double IntellisenseMinWidth
        {
            get => (double)GetValue(IntellisenseMinWidthProperty);
            set => SetValue(IntellisenseMinWidthProperty, value);
        }
        
        public static readonly DependencyProperty MaxRecodeProperty = DependencyProperty.Register(
            "MaxRecode", typeof(int), typeof(TextBoxIntellisenseBehavior), new PropertyMetadata(-1));

        public int MaxRecode
        {
            get { return (int)GetValue(MaxRecodeProperty); }
            set { SetValue(MaxRecodeProperty, value); }
        }

        private readonly ListBox _listBox = new ListBox();
        private readonly Popup _intellisense = new Popup();
        public TextBoxIntellisenseBehavior()
        {
            _listBox.PreviewMouseDown += (s, e) =>
            {
                var target = _listBox.InputHitTest(e.GetPosition(_listBox));
                if (target is DependencyObject dependencyObject)
                {
                    var item = dependencyObject.GetSelfAndAncestors().FirstOrDefault(x => x is ListBoxItem);
                    if (item is FrameworkElement frameworkElement)
                    {
                        _listBox.SelectedItem = frameworkElement.DataContext;
                    }
                    else
                    {
                        if (dependencyObject.GetSelfAndAncestors().FirstOrDefault(x => x is ScrollBar) != null)
                            return;
                    }
                }
                e.Handled = true;
            };
            VirtualizingPanel.SetScrollUnit(_listBox, ScrollUnit.Pixel);

            var itemStyle = new Style(typeof(ListBoxItem));
            itemStyle.Setters.Add(new EventSetter(Control.PreviewMouseDoubleClickEvent,
                new MouseButtonEventHandler(
                (s, e) =>
                {
                    if (s is FrameworkElement frameworkElement)
                    {
                        AssociatedObject.Text = frameworkElement.DataContext.ToString();
                        AssociatedObject.CaretIndex = AssociatedObject.Text.Length;
                        OpenIntellisense();
                        AssociatedObject.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                    }
                })));
            _listBox.ItemContainerStyle = itemStyle;

            _intellisense.StaysOpen = false;
            _intellisense.Child = _listBox;
        }

        protected override void OnAttached()
        {
            AssociatedObject.KeyDown += OnKeyDown;
            AssociatedObject.PreviewTextInput += OnPreviewTextInput;
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
            AssociatedObject.TextChanged += OnTextChanged;
            AssociatedObject.LostFocus += OnLostFocus;
            AssociatedObject.MouseDoubleClick += OnDoubleClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.KeyDown -= OnKeyDown;
            AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
            AssociatedObject.TextChanged -= OnTextChanged;
            AssociatedObject.LostFocus -= OnLostFocus;
            AssociatedObject.MouseDoubleClick -= OnDoubleClick;
        }

        private void TryOpenPopup()
        {
            if (ItemsSource?.Any() is true)
            {
                var defaultIndex = -1;
                var targetItems = ItemsSource.Where(item => item.ToLower().Contains(AssociatedObject.Text.ToLower())).Distinct().ToArray();

                if (targetItems.Length is 0)
                {
                    CloseIntellisense();
                    return;
                }

                _intellisense.PlacementTarget = AssociatedObject;
                OpenIntellisense();

                foreach (var target in targetItems.Select((item, index) => new { item, index }))
                {
                    if (target.item == AssociatedObject.Text)
                    {
                        defaultIndex = target.index;
                        break;
                    }
                }

                if (defaultIndex < 0 && targetItems.Any())
                    defaultIndex = 0;

                if (MaxRecode > 0)
                {
                    targetItems = targetItems.Take(MaxRecode).ToArray();
                }

                _listBox.ItemsSource = targetItems;
                _listBox.SelectedIndex = defaultIndex;
                if (defaultIndex >= 0)
                    _listBox.ScrollIntoView(_listBox.SelectedItem);
            }
            else
            {
                CloseIntellisense();
            }
        }

        private void OpenIntellisense()
        {
            _intellisense.IsOpen = true;
        }

        private void CloseIntellisense()
        {
            _intellisense.IsOpen = false;
        }

        private bool _isRequestIntellisense;
        private void RequestIntellisense()
        {
            _isRequestIntellisense = true;
        }

        private void CancelRequestIntellisense()
        {
            _isRequestIntellisense = false;
        }


        private void ApplySelectedText()
        {
            if (_listBox.SelectedItem != null)
            {
                AssociatedObject.Text = _listBox.SelectedItem.ToString();
                AssociatedObject.CaretIndex = AssociatedObject.Text.Length;
                CloseIntellisense();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Space)
            {
                TryOpenPopup();
                e.Handled = true;
            }
        }

        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TryOpenPopup();
            e.Handled = true;
        }

        private Dictionary<Key, Action> KeyboardActions { get; set; }

        private Dictionary<Key, Action> MakeKeyboardActions()
        {
            var dictionary = new Dictionary<Key, Action>();

            dictionary[Key.Down] = () =>
            {
                if (_listBox.SelectedIndex < ItemsSource.Count())
                {
                    _listBox.SelectedIndex++;
                    _listBox.ScrollIntoView(_listBox.SelectedItem);
                }
            };

            dictionary[Key.Up] = () =>
            {
                if (_listBox.SelectedIndex > 0)
                {
                    _listBox.SelectedIndex--;
                    _listBox.ScrollIntoView(_listBox.SelectedItem);
                }
            };

            dictionary[Key.Enter] =
            dictionary[Key.Tab] = ApplySelectedText;

            dictionary[Key.Escape] = CloseIntellisense;

            return dictionary;
        }

        private Action GetOrCreateKeyboardActions(Key key)
        {
            if (KeyboardActions is null)
                KeyboardActions = MakeKeyboardActions();

            if (KeyboardActions.ContainsKey(key))
                return KeyboardActions[key];

            return null;
        }


        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_intellisense.IsOpen)
            {
                var action = GetOrCreateKeyboardActions(e.Key);

                if (action != null)
                {
                    action.Invoke();
                    e.Handled = true;
                }
            }
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            RequestIntellisense();
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            CloseIntellisense();
        }

        private bool _isInitText = true;
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitText)
                _isInitText = false;
            else
                RequestIntellisense();

            if (_isRequestIntellisense)
            {
                if (string.IsNullOrEmpty(AssociatedObject.Text))
                {
                    CloseIntellisense();
                    CancelRequestIntellisense();
                    _isInitText = true;
                }
                else
                {
                    TryOpenPopup();
                }
                CancelRequestIntellisense();
            }
        }
    }
}
