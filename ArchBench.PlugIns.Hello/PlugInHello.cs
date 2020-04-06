using HttpServer;
using HttpServer.Sessions;
using System;
using System.IO;

namespace ArchBench.PlugIns.Hello
{
    public class PlugInHello : IArchBenchHttpPlugIn
    {
        public string Name => "PlugIn Hello";

        public string Description => "Responde 'Hi!', sempre que recebe um pedido de '/hello'";

        public string Author => "Leonel Nóbrega";

        public string Version => "1.0";

        public bool Enabled { get; set; } = false;

        public IArchBenchPlugInHost Host { get; set; }

        public IArchBenchSettings Settings { get; } = new ArchBenchSettings();

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public bool Process(IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            if ( aRequest.Uri.AbsolutePath.StartsWith( "/hello", StringComparison.InvariantCultureIgnoreCase ) )
            {
                var writer = new StreamWriter(aResponse.Body);
                writer.WriteLine("Hi!");
                writer.Flush();

                return true;
            }
            return false;
        }
    }
}
