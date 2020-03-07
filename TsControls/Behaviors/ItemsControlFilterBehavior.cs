using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace TsControls.Behaviors
{
    public interface ITriggerMessageAction
    {
        void Invoke();
    }
    
    public class ItemsControlFilterBehavior : Behavior<ItemsControl> , ITriggerMessageAction
    {
        public static readonly DependencyProperty FilterActionPathProperty =
            DependencyProperty.Register(
                nameof(FilterActionPath),
                typeof(string),
                typeof(ItemsControlFilterBehavior),
                new PropertyMetadata(default));

        public string FilterActionPath
        {
            get => (string)GetValue(FilterActionPathProperty);
            set => SetValue(FilterActionPathProperty, value);
        }

        private ICollectionView _collectionView; 

        protected override void OnAttached()
        {
            base.OnAttached();
            _collectionView = CollectionViewSource.GetDefaultView(AssociatedObject.ItemsSource);

            var dataContext = AssociatedObject.DataContext;

            var function = dataContext
                .GetType()
                .GetMethod(FilterActionPath)
                ?.CreateDelegate(typeof(Func<object, bool>), dataContext) as Func<object, bool>;

            _collectionView.Filter += (e) => function != null && function(e);
        }

        public void Invoke()
        {
            _collectionView.Refresh();
        }
    }
}