using System;

namespace ArchBench.PlugIns.Utils.RequestResponse
{

    public class RequestResponse : Observer.Observable<IRequestResponseEndpoint>
    {

        public bool Emit(string aEndpoint, IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            foreach (var subscriber in _subscribers)
            {
                if (aEndpoint.Equals(subscriber.Endpoint))
                    return subscriber.Handler.Invoke(aRequest, aResponse, aSession);
            }

            foreach (var subscriber in _subscribers)
            {
                if (subscriber.Endpoint.Equals("*"))
                    return subscriber.Handler.Invoke(aRequest, aResponse, aSession);
            }
            return false;
        }
    }
}