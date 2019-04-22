using System.ComponentModel;

namespace TsControls.EventListeners
{
    public class PropertyChangedListener : SingleEventListener<INotifyPropertyChanged, PropertyChangedEventHandler>
    {
        public PropertyChangedListener(INotifyPropertyChanged collectionChanged) : base(collectionChanged)
        {
        }

        public PropertyChangedListener(INotifyPropertyChanged collectionChanged,
            PropertyChangedEventHandler eventHandler) : base(collectionChanged,eventHandler)
        {
        }

        protected override void AddEventInternal(INotifyPropertyChanged subject, PropertyChangedEventHandler e)
        {
            subject.PropertyChanged += e;
        }

        protected override void RemoveEventInternal(INotifyPropertyChanged subject, PropertyChangedEventHandler e)
        {
            subject.PropertyChanged -= e;
        }
    }
}
