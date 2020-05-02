using System;
using System.Collections.Generic;
using ArchBench.PlugIns.Utils.Session;

namespace ArchBench.PlugIns.Broker.Strategies
{
    public class SameServerStrategy : IServerDispatcherStrategy
    {
    
        private IServerDispatcherStrategy _newClientStrategy;
        private Session _session;

        public SameServerStrategy(IServerDispatcherStrategy newClientStrategy, Session aSession)
        {
            _newClientStrategy = newClientStrategy;
            _session = aSession;
        }

        public int GetNextServer()
        {

                if (!_session.Vars.ContainsKey("server")) _session.Vars["server"] = _newClientStrategy.GetNextServer();

                return (int)_session.Vars["server"];



        }
    }
}
