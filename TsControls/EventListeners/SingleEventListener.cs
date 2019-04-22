using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TsControls.EventListeners
{
    public abstract class SingleEventListener<TSubject,TEventHandler> : IDisposable
    {
        private readonly TSubject _subject;
        private readonly List<TEventHandler> _events = new List<TEventHandler>();
        protected SingleEventListener(TSubject subject)
        {
            Debug.Assert(subject != null);
            _subject = subject;
        }

        protected SingleEventListener(TSubject subject, TEventHandler eventHandler) : this(subject)
        {
            Add(eventHandler);
        }

        public void Add(TEventHandler eventHandler)
        {
            AddEventInternal(_subject, eventHandler);
            _events.Add(eventHandler);
        }

        public void Add(TEventHandler eventHandler,params TEventHandler[] events)
        {
            Add(eventHandler);
            foreach (var e in events)
                Add(e);
        }

        public void Dispose()
        {
            foreach (var e in _events)
                RemoveEventInternal(_subject,e);
        }
        protected abstract void AddEventInternal(TSubject subject ,  TEventHandler e) ;
        protected abstract void RemoveEventInternal(TSubject subject, TEventHandler e) ;
    }
}