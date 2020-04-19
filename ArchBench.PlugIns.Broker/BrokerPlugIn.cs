using HttpServer;
using HttpServer.Sessions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace ArchBench.PlugIns.Broker
{
    public class BrokerPlugIn : IArchBenchHttpPlugIn
    {
        public string Name        => @"Broker PlugIn";
        public string Description => @"Implements HTTP Broker";
        public string Author      => @"Leonel Nóbrega";
        public string Version     => @"1.0";

        public bool Enabled { get; set; } = false;
        public IArchBenchPlugInHost Host { get; set; }

        public IArchBenchSettings Settings { get; } = new ArchBenchSettings();

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        private Registry Registry { get; } = new Registry();
        private Assignments<string> Assignments { get; } = new Assignments<string>();
    
        public bool Process(IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            if ( aRequest.Uri.AbsolutePath.StartsWith( "/register" ) )
            {
                aResponse.Status = HttpStatusCode.Accepted;

                var url = $"http://{aRequest.Headers[ "remote_addr" ]}:{ aRequest.QueryString["port"].Value }";
                if ( Registry.Append( url ) )
                {
                    Host.Logger.WriteLine( $"Server at '{ url}' added." );
                }

                return true;
            }
            else if ( aRequest.Uri.AbsolutePath.StartsWith( "/unregister" ) )
            {
                aResponse.Status = HttpStatusCode.Accepted;

                var url = $"http://{aRequest.Headers["remote_addr"]}:{ aRequest.QueryString["port"].Value }";
                if ( Registry.Remove( url ) )
                {
                    Host.Logger.WriteLine($"Server at '{ url}' removed.");
                }

                return true;
            }
            else
            {
                var cookie = GetCookie( aRequest );
                var server = ( cookie != null ? Assignments[ cookie ] : null ) ?? Registry.Get();

                Host.Logger.WriteLine($"Forward to { server }");
                DownloadPageContents( aRequest, aResponse, server );
                aResponse.Send();

                if ( cookie != null ) Assignments[ cookie ] = server;
            }

            return false;
        }

        private void DownloadPageContents( HttpServer.IHttpRequest aRequest, HttpServer.IHttpResponse aResponse, string aHost )
        {
            if ( string.IsNullOrEmpty( aHost ) ) return;

            var uri = new Uri( aHost + aRequest.Uri.AbsolutePath );
            var client = new WebClient { QueryString = GetFormValues( aRequest ) };

            ForwardCookie( aRequest, client );

            byte[] bytes = null;
            if ( aRequest.Method == Method.Post )
            {
                bytes = client.UploadValues( uri, GetFormValues(aRequest));
            }
            else
            {
                bytes = client.DownloadData( uri );
            }

            BackwardCookie( aResponse, client );

            if ( client.ResponseHeaders.AllKeys.Contains( "Set-Cookie" ) )
            {
                var parts = client.ResponseHeaders[ "Set-Cookie" ].Split( ';' );
                Assignments[ parts[0] ] = aHost;
            }

            aResponse.Body.Write( bytes, 0, bytes.Length );
        }

        private NameValueCollection GetFormValues(IHttpRequest aRequest)
        {
            var values = new NameValueCollection();
            foreach ( HttpInputItem item in aRequest.Form )
            {
                values.Add(item.Name, item.Value);
            }
            return values;
        }

        private string GetQueryString( IHttpRequest aRequest )
        {
            int count = aRequest.QueryString.Count();
            if (count == 0) return "";

            var parameters = new StringBuilder("?");
            foreach (HttpInputItem item in aRequest.QueryString)
            {
                parameters.Append( $"{ item.Name }={ item.Value }" );
                if (--count > 0) parameters.Append('&');
            }
            return parameters.ToString();
        }

        private string GetCookie( IHttpRequest aRequest )
        {
            var cookie = aRequest.Headers[ "Cookie" ];
//            Host.Logger.WriteLine( $"Request Cookie : { cookie ?? "none" }" );
            return aRequest.Headers["Cookie"];
        }

        private string GetCookie( IHttpResponse aResponse)
        {
            if ( aResponse.Cookies.Count == 0 ) return null;
            return aResponse.Cookies.FirstOrDefault()?.Value;
        }
        
        private void ForwardCookie( IHttpRequest aRequest, WebClient aClient )
        {
            if (aRequest.Headers["Cookie"] == null) return;
            aClient.Headers.Add("Cookie", aRequest.Headers["Cookie"]);
        }

        private void BackwardCookie( IHttpResponse aResponse, WebClient aClient )
        {
            //if (aClient.ResponseHeaders["Set-Cookie"] == null) return;
            //aResponse.AddHeader("Set-Cookie", aClient.ResponseHeaders["Set-Cookie"]);

            //foreach ( string header in aClient.Headers )
            //{
            //    if ( header != "Cookie" ) continue;

            //}

            foreach ( string header in aClient.ResponseHeaders )
            {
                aResponse.AddHeader( header, aClient.ResponseHeaders[header] );
            }
        }
    }
}
