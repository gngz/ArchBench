using HttpServer;
using HttpServer.Sessions;
using System;

namespace ArchBench.PlugIns.Utils.RequestResponse
{
    public interface IRequestResponseEndpoint
    {
        string Endpoint { get; set; }
        Func<IHttpRequest, IHttpResponse, IHttpSession, bool> Handler { get; set; }
    }
}