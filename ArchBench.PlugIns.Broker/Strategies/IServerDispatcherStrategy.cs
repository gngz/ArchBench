using System;
namespace ArchBench.PlugIns.Broker.Strategies
{
    public interface IServerDispatcherStrategy
    {
        int GetNextServer();
    }
}
