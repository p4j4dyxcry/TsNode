using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace WpfApp1
{
    public class UndoRedoKeyBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty UndoCommandProperty = DependencyProperty.Register(
            nameof(UndoCommand), typeof(ICommand), typeof(UndoRedoKeyBehavior), new PropertyMetadata(default(ICommand)));

        public ICommand UndoCommand
        {
            get => (ICommand)GetValue(UndoCommandProperty);
            set => SetValue(UndoCommandProperty, value);
        }

        public static readonly DependencyProperty RedoCommandProperty = DependencyProperty.Register(
            nameof(RedoCommand), typeof(ICommand), typeof(UndoRedoKeyBehavior), new PropertyMetadata(default(ICommand)));

        public ICommand RedoCommand
        {
            get => (ICommand)GetValue(RedoCommandProperty);
            set => SetValue(RedoCommandProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.KeyDown += (s, e) =>
            {
                if(e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
                {
                    if(UndoCommand?.CanExecute(null) is true)
                        UndoCommand?.Execute(null);
                    e.Handled = true;
                }

                else if (e.Key == Key.Y && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
                {
                    if (RedoCommand?.CanExecute(null) is true)
                        RedoCommand?.Execute(null);
                    e.Handled = true;
                }
            };
        }
    }
}
