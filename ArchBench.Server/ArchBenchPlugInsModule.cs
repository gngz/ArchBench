using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;
using ArchBench.PlugIns;

namespace ArchBench.Server
{
    public class ArchBenchPlugInsModule : HttpModule, IArchBenchPlugInHost
    {
        public ArchBenchPlugInsModule( IArchBenchLogger aLogger )
        {
            this.Logger = aLogger;
            PlugInManager = new PlugInManager( this );
        }

        public PlugInManager PlugInManager { get; private set; }

        public override bool Process( IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession )
        {
            foreach ( IArchBenchModulePlugIn plugin in PlugInManager.PlugIns )
            {
                if ( ! plugin.Process( aRequest, aResponse, aSession ) ) continue;
                Logger.WriteLine( "Plugin {0} processed request {1}", plugin.Name, aRequest.Uri.ToString() );
                return true;
            }
            return false;
        }

        #region IArchServerPlugInHost Members

        public IArchBenchLogger Logger
        {
            get; set;
        }

        #endregion
    }
}
