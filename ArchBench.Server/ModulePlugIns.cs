using ArchBench.PlugIns;
using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;

namespace ArchBench.Server
{
    public class ModulePlugIns : HttpModule, IArchBenchPlugInHost
    {
        public ModulePlugIns( IArchBenchLogger aLogger )
        {
            Logger = aLogger;
            PlugInsManager = new PlugInsManager( this );
        }

        public IArchBenchLogger Logger { get; set; }

        public PlugInsManager PlugInsManager { get; }

        public override bool Process( IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession )
        {
            foreach ( var plugin in PlugInsManager.PlugIns )
            {
                if ( ! plugin.Enabled ) continue;
                if ( ! ( plugin is IArchBenchHttpPlugIn httpPlugIn ) ) continue;

                if ( httpPlugIn.Process( aRequest, aResponse, aSession ) ) return true;
            }
            return false;
        }
    }
}
