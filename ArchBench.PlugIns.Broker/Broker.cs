using ArchBench.PlugIns.Broker.Strategies;
using ArchBench.PlugIns.Utils.RequestResponse;
using ArchBench.PlugIns.Utils.Session;
using HttpServer;
using HttpServer.Sessions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ArchBench.PlugIns.Broker
{
    public class Broker : IArchBenchHttpPlugIn
    {
        #region Plugin Metadata

        public string Name => "Broker Plugin";
        public string Description => "Broker";
        public string Author => "Gonçalo Passos";
        public string Version => "0.9";

        #endregion

        public bool Enabled { get; set; }
        public IArchBenchPlugInHost Host { get; set; }
        private IArchBenchLogger Logger { get; set; }
        public IArchBenchSettings Settings { get; set; } = new ArchBenchSettings();

        private IList<string> _servers = new List<string>();

        private RequestResponse _requestResponse = new RequestResponse();

        public void Dispose()
        {

        }

        public void Initialize()
        {
            Logger = this.Host.Logger;
            _requestResponse.Subscribe(new RequestResponseEndpoint { Endpoint = "/broker/register", Handler = RegisterServer });
            _requestResponse.Subscribe(new RequestResponseEndpoint { Endpoint = "/broker/unregister", Handler = UnregisterServer });
            _requestResponse.Subscribe(new RequestResponseEndpoint { Endpoint = "*", Handler = ProcessRequest });

            this.Settings["Algorithim"] = "roundrobin";
        }

        public bool Process(IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            // TODO INFORMATION
            Logger.WriteLine(aRequest.UriPath);

            return _requestResponse.Emit(aRequest.UriPath, aRequest, aResponse, aSession);
        }

        private bool RegisterServer(IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            var reader = new StreamReader(aRequest.Body);
            var address = reader.ReadToEnd();
            _servers.Add(address);
            Logger.WriteLine($"Server {address} added!");

            return true;

        }

        private bool UnregisterServer(IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            var reader = new StreamReader(aRequest.Body);
            var address = reader.ReadToEnd();
            _servers.Remove(address);
            Logger.WriteLine($"Server {address} removed!");

            return true;

        }

        private IServerDispatcherStrategy ChooseStrategy(Session aSession)
        {
            IServerDispatcherStrategy roundrobin = new RoundrobinStrategy(_servers);
            switch (this.Settings["Algorithim"])
            {
                case "roundrobin": return roundrobin;
                case "sameserver": return new SameServerStrategy(roundrobin, aSession);
                default: return roundrobin;
            }
        }

        private bool ProcessRequest(IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            //   int retrys = 0;
            Session session = new Session("_broker_session");
            session.HandleSession(aRequest, aResponse);

            IServerDispatcherStrategy serverDispatcher = ChooseStrategy(session);

            int index = serverDispatcher.GetNextServer();

            if (index == -1) return false;


            HttpWebRequest request = PrepareServerRequest(aRequest, index);


            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                PrepareClientResponse(aResponse, response);
                SetResponseCookies(response, aResponse, index, aRequest.Uri.Host);
                HandleResponse(response, aResponse, aRequest, _servers[index]);

            }
            catch (WebException e) when (e.Status == WebExceptionStatus.Timeout)
            {
                _servers.RemoveAt(index);
            }
            catch (WebException exception)
            {
                if (exception.Response != null)
                {
                    HttpWebResponse response = (HttpWebResponse)exception.Response;
                    PrepareClientResponse(aResponse, response);
                    SetResponseCookies(response, aResponse, index, aRequest.Uri.Host);
                    HandleResponse(response, aResponse, aRequest, _servers[index]);
                }
                else
                {
                    aResponse.Status = HttpStatusCode.InternalServerError;
                    StreamWriter writer = new StreamWriter(aResponse.Body);
                    writer.Write(exception.Message);
                    writer.Flush();
                }
            }
            return true;
        }


        private void HandleResponse(HttpWebResponse serverResponse, IHttpResponse clientResponse, IHttpRequest clientRequest, string serverAddress)
        {
            Stream stream = serverResponse.GetResponseStream();

            if (serverResponse.ContentType.Equals(ContentType.Html))
            {
                StreamReader streamReader = new StreamReader(stream);
                StreamWriter streamWriter = new StreamWriter(clientResponse.Body);
                streamWriter.Write(ParseLinks(streamReader.ReadToEnd(), serverAddress, clientRequest.Uri));
                streamWriter.Flush();
            }
            else
            {
                stream.CopyTo(clientResponse.Body);
            }

            serverResponse.Close();
        }

        private void PrepareClientResponse(IHttpResponse clientResponse, HttpWebResponse serverResponse)
        {
            clientResponse.Status = serverResponse.StatusCode;
            clientResponse.ContentType = serverResponse.ContentType;
        }


        private HttpWebRequest PrepareServerRequest(IHttpRequest clientRequest, int aServer)
        {
            HttpWebRequest request = WebRequest.CreateHttp("http://" + _servers[aServer] + clientRequest.UriPath);
            String contentType = clientRequest.Headers["Content-Type"];


            request.ContentLength = clientRequest.ContentLength;
            request.Method = clientRequest.Method;
            request.UserAgent = clientRequest.Headers["User-Agent"];
            request.CookieContainer = new CookieContainer();

            // request.Headers = aRequest.Headers; TODO dá erro por causa do user Agent
            /*
            foreach (var headerKey in aRequest.Headers.AllKeys)
            {
                request.Headers.Add(headerKey, aRequest.Headers[headerKey]);
              
            }*/


            if (contentType != null) request.ContentType = clientRequest.Headers["Content-Type"];

            if (clientRequest.Method != Method.Get)
            {
                if (Form.IsUrlEncoded(contentType))
                {
                    Form.SendForm(clientRequest.Form, request);
                }
                else if(Form.IsMultipart(contentType))
                {
                    Form.SendMultipart(clientRequest.Form, request);
                }
                else
                {
                    clientRequest.Body.CopyTo(request.GetRequestStream());
                }

            }

            SetRequestCookies(clientRequest, request, aServer);


            return request;
        }

        private void SetRequestCookies(IHttpRequest clientRequest, HttpWebRequest serverRequest, int aServer)
        {
            if (clientRequest.Cookies.Count > 0)
            {
                var cookieEnum = clientRequest.Cookies.GetEnumerator();

                while (cookieEnum.MoveNext())
                {
                    RequestCookie cookie = (HttpServer.RequestCookie)cookieEnum.Current;
                    serverRequest.CookieContainer.Add(new Cookie(cookie.Name, cookie.Value) { Domain = new Uri($"http://{_servers[aServer]}").Host });
                }
            }
        }

        private void SetResponseCookies(HttpWebResponse serverResponse, IHttpResponse clientResponse, int aServer, string aDomain)
        {
            foreach (Cookie cook in serverResponse.Cookies)
            {
                if (cook.Domain.Equals(new Uri($"http://{_servers[aServer]}").Host))
                {
                    cook.Domain = aDomain;
                }

                clientResponse.Cookies.Add(CookieAdapter.adaptFromCookie(cook));

            }
        }



        private string ParseLinks(string aBody, string aServer, Uri aAddress)
        {
            var address = aAddress.Port == 80 ? aAddress.Host : $"{aAddress.Host}:{aAddress.Port}";
            var serverPort = new Uri($"http://{aServer}").Port;

            if (serverPort == 80)
            {
                return aBody.Replace(aServer, address)
                .Replace($"127.0.0.1", address)
                .Replace($"localhost", address);
            }
            else
            {
                return aBody.Replace(aServer, address)
                .Replace($"127.0.0.1:{serverPort}", address)
                .Replace($"localhost:{serverPort}", address);
            }

        }
    }
}
