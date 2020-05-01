using System;
using System.Collections.Generic;

namespace ArchBench.PlugIns.Utils.Observer
{
    public abstract class Observable<T> : Observer.IObservable<T>
    {
        protected List<T> _subscribers = new List<T>();

        public void Subscribe(T aSubscriber)
        {
            if (!_subscribers.Contains(aSubscriber))
            {
                _subscribers.Add(aSubscriber);
            }
        }

        public void Unsubscribe(T aSubscriber)
        {
            _subscribers.Remove(aSubscriber);
        }
    }
}
