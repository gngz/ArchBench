using System.Collections.Generic;
using System.Net;
using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;
using ArchBench.PlugIns;

namespace ArchBench.Server.Kernel
{
    public class PlugInsServer : HttpModule, IArchBenchPlugInHost
    {
        public IArchBenchLogger Logger  { get; }
        public IPlugInsManager  Manager { get; }

        public PlugInsServer( IArchBenchLogger aLogger = null )
        {
            Logger  = aLogger ?? new ConsoleLogger();
            Manager = new PlugInsManager( this );
        }

        public HttpServer.HttpServer HttpServer { get; private set; }

        public override bool Process(IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            foreach (var plugin in Manager.PlugIns)
            {
                if (!plugin.Enabled) continue;
                if (!(plugin is IArchBenchHttpPlugIn httpPlugIn)) continue;

                if (httpPlugIn.Process(aRequest, aResponse, aSession)) return true;
            }
            return false;
        }

        public void Start( int aPort )
        {
            HttpServer = new HttpServer.HttpServer();
            HttpServer.Add( this );
            HttpServer.Start( IPAddress.Any, aPort );
        }

        public void Stop()
        {
            HttpServer?.Stop();
            HttpServer = null;
        }

        public IEnumerable<IArchBenchPlugIn> Install( string aFileName )
        {
            return Manager.Add( aFileName );
        }

        public void Enable( IArchBenchPlugIn aPlugIn, bool aEnabled )
        {
            if ( aPlugIn != null ) aPlugIn.Enabled = aEnabled;
        }
    }
}
