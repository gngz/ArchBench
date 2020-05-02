using System;
using System.Collections.Generic;

namespace ArchBench.PlugIns.Broker.Strategies
{
    public class RoundrobinStrategy : IServerDispatcherStrategy
    {


        private IList<string> _servers;
        private int _currentServer = 0;
        private static RoundrobinStrategy _instance;

        private RoundrobinStrategy(IList<string> serverList)
        {
            _servers = serverList;
        }

        public static RoundrobinStrategy GetInstance(IList<string> serverList)
        {
            if (_instance == null)
            {
                _instance = new RoundrobinStrategy(serverList);
            }



            return _instance;

        }

        public int GetNextServer()
        {
            if (_servers.Count == 0) return -1;
            _currentServer = (_currentServer + 1) % _servers.Count;
            return _currentServer;
        }
    }
}
