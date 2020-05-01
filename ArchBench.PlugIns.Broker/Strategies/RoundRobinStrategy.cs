using System;
using System.Collections.Generic;

namespace ArchBench.PlugIns.Broker.Strategies
{
    public class RoundrobinStrategy : IServerDispatcherStrategy
    {


        private IList<string> _servers;
        private int _currentServer = 0;

        public RoundrobinStrategy(IList<string> serverList)
        {
            _servers = serverList;
        }

        public int GetNextServer()
        {
            if (_servers.Count == 0) return -1;
            _currentServer = (_currentServer + 1) % _servers.Count;
            return _currentServer;
        }
    }
}
