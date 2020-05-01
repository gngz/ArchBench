using HttpServer;
using HttpServer.Sessions;
using System;
using System.IO;
using System.Text;

namespace ArchBench.PlugIns.RequestTester
{
    public class RequestTester : IArchBenchHttpPlugIn
    {
        public string Name => "Request Tester";

        public string Description => "Display Requests Information";

        public string Author => "Gonçalo Passos";

        public string Version => "0.9";

        public bool Enabled { get; set; }
        public IArchBenchPlugInHost Host { get; set; }

        public IArchBenchSettings Settings => new ArchBenchSettings();

        public void Dispose() { }

        public void Initialize() { }

        public bool Process(IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            StreamWriter writer = new StreamWriter(aResponse.Body);

            var method = aRequest.Method;
            var uriPath = aRequest.UriPath;

            writer.WriteLine($"<h1>{method} {uriPath} </h1>");

            displayHeaders(aRequest,writer);
            displayCookies(aRequest, writer);
            displayFormData(aRequest, writer);
            displayFormFiles(aRequest, writer);
            displayBody(aRequest, writer);

            writer.Flush();
         



            return true;
        }

        public void displayHeaders(IHttpRequest aRequest , StreamWriter aWriter)
        {
            aWriter.WriteLine($"<h2>Request Headers</h2>");

            foreach(string header in aRequest.Headers.Keys)
            {
                var value = aRequest.Headers[header];
                aWriter.WriteLine($"<p><strong>{header}:</strong> {value}</p>");
            }
           
       
        }

        public void displayBody(IHttpRequest aRequest, StreamWriter aWriter)
        {
            byte[] buffer = new byte[aRequest.Body.Length];
            if (aRequest.Body.Read(buffer, 0, buffer.Length) > 0)
            {
                aWriter.WriteLine($"<h2>Body</h2>");
                aWriter.WriteLine(Encoding.ASCII.GetString(buffer));
                aWriter.WriteLine($"<br>");

            }
       
        }

        public void displayCookies(IHttpRequest aRequest, StreamWriter aWriter)
        {
            if (aRequest.Cookies.Count > 0)
            {
                aWriter.WriteLine($"<h2>Cookies</h2>");

                foreach (RequestCookie cookie in aRequest.Cookies)
                {
                    aWriter.WriteLine($"<p><strong>{cookie.Name}:</strong> {cookie.Value}</p>");
                }
            }
        }

        public void displayFormData(IHttpRequest aRequest, StreamWriter aWriter)
        {
            var formData = aRequest.Form.ToString();

            if(!string.IsNullOrEmpty(formData))
            {
                foreach(HttpInputItem input in aRequest.Form)
                {
                    aWriter.WriteLine($"<p><strong>{input.Name}:</strong> {input.Value}</p>");
                }
            }

        }

        public void displayFormFiles(IHttpRequest aRequest, StreamWriter aWriter)
        {
            if(aRequest.Form.Files.Count > 0)
            {
                aWriter.WriteLine($"<h2>Form Files</h2>");

                foreach(HttpFile file in aRequest.Form.Files)
                {
                    
                    aWriter.WriteLine($"<p><strong>Field Name:</strong> {file.Name}</p>");
                    aWriter.WriteLine($"<p><strong>Content Type:</strong> {file.ContentType}</p>");
                    aWriter.WriteLine($"<p><strong>Client Directory:</strong> {file.Filename}</p>");
                    aWriter.WriteLine($"<p><strong>Name:</strong> {file.UploadFilename}</p>");
                    aWriter.WriteLine($"<hr>");
                }

            }
           
        }





    }
}
