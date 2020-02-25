using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace TsControls.Behaviors
{
    public class BindableBehavior<T> : Behavior<T> where T : DependencyObject
    {
        public static readonly DependencyProperty BindingProperty =
            DependencyProperty.Register(
                nameof(Binding),
                typeof(Behavior<T>), 
                typeof(BindableBehavior<T>),
                new PropertyMetadata(default));

        public Behavior<T> Binding
        {
            get => (Behavior<T>)GetValue(BindingProperty);
            set => SetValue(BindingProperty, value);
        }

        protected override void OnAttached()
        {
            Binding.Attach(AssociatedObject);
        }

        protected override void OnDetaching()
        {
            Binding?.Detach();
        }
    }

    public class ItemsControlBindableBehavior : BindableBehavior<ItemsControl>
    {

    }
}