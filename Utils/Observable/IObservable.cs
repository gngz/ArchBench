using System;

namespace ArchBench.PlugIns.Utils.Observer
{
    public interface IObservable<T>
    {
        void Subscribe(T aSubscriber);
        void Unsubscribe(T aSubscriber);
    }
}
