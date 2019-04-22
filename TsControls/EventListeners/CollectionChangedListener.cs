using System.Collections.Specialized;

namespace TsControls.EventListeners
{
    public class CollectionChangedListener : SingleEventListener<INotifyCollectionChanged, NotifyCollectionChangedEventHandler>
    {
        public CollectionChangedListener(INotifyCollectionChanged collectionChanged):base(collectionChanged)
        {
        }

        public CollectionChangedListener(INotifyCollectionChanged collectionChanged, 
            NotifyCollectionChangedEventHandler eventHandler) :base(collectionChanged)
        {
        }

        protected override void AddEventInternal(INotifyCollectionChanged subject, NotifyCollectionChangedEventHandler e)
        {
            subject.CollectionChanged += e;
        }

        protected override void RemoveEventInternal(INotifyCollectionChanged subject, NotifyCollectionChangedEventHandler e)
        {
            subject.CollectionChanged -= e;
        }
    }
}
