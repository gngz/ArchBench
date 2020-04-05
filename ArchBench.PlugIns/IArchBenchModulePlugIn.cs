using HttpServer;
using HttpServer.Sessions;

namespace ArchBench.PlugIns
{
    public interface IArchBenchModulePlugIn : IArchBenchPlugIn
    {
        bool Process( IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession );
    }
}
