using System;
using System.Collections.Generic;
using System.Text;

namespace ArchBench.PlugIns.Broker
{
    internal class Assignments<T> 
    {
        private IDictionary<string, Entry<T>> Entries { get; }  = new Dictionary<string, Entry<T>>();

        public T this[ string key] 
        {
            get
            {
                if ( key == null ) return default(T);
                if ( ! Entries.ContainsKey( key ) ) return default(T);
                return Entries[ key ].Value;
            }
            set
            {
                if ( Entries.ContainsKey( key ) )
                {
                    Entries[ key ].Value = value;
                }
                else
                {
                    Console.WriteLine( $"ASSIGN {key} to {value}");
                    Entries.Add( key, new Entry<T>( value ) );                    
                }
                Entries[key].TimeStamp = DateTime.Now;
            }
        }
    }
}
