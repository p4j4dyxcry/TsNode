using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace TsControls.Behaviors
{
    public class DoubleClickBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(DoubleClickBehavior), new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(DoubleClickBehavior), new PropertyMetadata(null));
        public object CommandParameter
        {
            get => (object)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void OnAttached()
        {
            this.AssociatedObject.MouseLeftButtonDown += OnMouseButtonDown;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseLeftButtonDown -= OnMouseButtonDown;
        }

        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
                return;

            if (this.Command == null)
                return;

            if (this.Command.CanExecute(this.CommandParameter))
            {
                this.Command.Execute(this.CommandParameter);
            }
        }
    }
}