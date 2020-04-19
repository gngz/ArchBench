using System;
using System.Collections.Generic;
using System.Text;

namespace ArchBench.PlugIns.Broker
{
    public class Registry
    {
        private int Next { get; set;} = -1;
        private IList<string> Servers { get; } = new List<string>();

        public bool Append( string aUrl )
        {
            if ( Servers.Contains( aUrl ) ) return false;

            Servers.Add( aUrl );
            return true;
        }

        public bool Remove( string aUrl )
        {
            if ( ! Servers.Contains( aUrl ) ) return false;

            Servers.Remove( aUrl );
            if ( Servers.Count == 0 ) Next = -1;
            return true;
        }

        public string Get()
        {
            if ( Servers.Count == 0 ) return string.Empty;

            Next = (Next + 1) % Servers.Count;
            return Servers[ Next ];
        }
    }
}
