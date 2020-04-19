using System;
using System.Collections.Generic;
using System.Text;

namespace ArchBench.PlugIns.Broker
{
    internal class Entry<T>
    {
        public Entry( T value = default(T) )
        {
            Value = value;
        }

        public T Value { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
