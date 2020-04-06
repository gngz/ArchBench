using System;
using System.IO;
using HttpServer;
using HttpServer.Sessions;

namespace ArchBench.PlugIns.Hello
{
    public class PlugInHello : IArchBenchHttpPlugIn
    {
        public string Name { get; } = "Hello PlugIn";
        public string Description { get; } = "Este plugin responde com 'Hi', sempre que recebe um pedido de /hello";
        public string Author { get; } = "Leonel Nóbrega";
        public string Version { get; } = "1.0";

        public bool Enabled { get; set; } = false;

        public IArchBenchPlugInHost Host { get; set; }
        
        public IArchBenchSettings Settings { get; } = new ArchBenchSettings();

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public bool Process( IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession )
        {
            if (aRequest.Uri.AbsolutePath.StartsWith( "/hello", StringComparison.InvariantCultureIgnoreCase ) )
            {
                var writer = new StreamWriter( aResponse.Body );
                writer.WriteLine( "Hi!" );
                writer.Flush();

                return true;
            }
            return false;
        }
    }
}
