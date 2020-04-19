using System;
using System.Runtime.CompilerServices;
using ArchBench.PlugIns;
using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;

namespace ArchBench.Server
{
    public class ModulePlugIns : HttpModule, IArchBenchPlugInHost
    {
        public ModulePlugIns( IArchBenchLogger aLogger, Func<Uri> aGetUri )
        {
            Logger  = aLogger;
            UriHandler = aGetUri;

            Manager = new PlugInsManager( this );
        }

        private Func<Uri> UriHandler { get; }

        public Uri Uri => UriHandler();
        public IArchBenchLogger Logger { get; set; }

        public IPlugInsManager Manager { get; }

        public override bool Process( IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession )
        {
            foreach ( var plugin in Manager.PlugIns )
            {
                if ( ! plugin.Enabled ) continue;
                if ( ! ( plugin is IArchBenchHttpPlugIn httpPlugIn ) ) continue;

                if ( httpPlugIn.Process( aRequest, aResponse, aSession ) ) return true;
            }
            return false;
        }
    }
}
