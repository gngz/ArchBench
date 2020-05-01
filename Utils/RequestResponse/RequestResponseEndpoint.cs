using HttpServer;
using HttpServer.Sessions;
using System;

namespace ArchBench.PlugIns.Utils.RequestResponse
{
    public class RequestResponseEndpoint : IRequestResponseEndpoint
    {
        public string Endpoint { get; set; }
        public Func<IHttpRequest, IHttpResponse, IHttpSession, bool> Handler { get; set; }
    }
}