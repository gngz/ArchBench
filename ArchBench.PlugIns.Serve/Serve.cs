using HttpServer;
using HttpServer.Sessions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ArchBench.PlugIns.Serve
{
    public class Serve : IArchBenchHttpPlugIn
    {
        #region PlugIn Metadata

        public string Name => "Serve";

        public string Description => "Static File Serve for ArchBench";

        public string Author => "Gonçalo Passos";

        public string Version => "0.9";

        #endregion

        public bool Enabled { get; set; }

        public IArchBenchPlugInHost Host { get; set; }

        public IArchBenchSettings Settings { get; set; } = new ArchBenchSettings();

        public void Dispose()
        {

        }

        public void Initialize()
        {
            Settings["folder"] = @"/home/gngz/site";
            Settings["log"] = "false";
        }
        public bool Process(IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            var logger = this.Host.Logger;
            var requestedUri = aRequest.UriPath.Remove(0, 1);



            var baseFolder = Settings["folder"];
            var completePath = Path.Combine(baseFolder, requestedUri);

            logger.WriteLine("Processing a request");
            logger.WriteLine($"PATH: {baseFolder}");
            logger.WriteLine($"URI PATH: {requestedUri}");
            logger.WriteLine($"Complete PATH: {completePath}");

            aResponse.AddHeader("X-Powered-By", "Serve for ArchBench");


            if (aRequest.Method != Method.Get)
            {
                MethodNotAllowed(aResponse);
                return false;
            }

            if (Directory.Exists(completePath))
            {
                if (!requestedUri.EndsWith("/", StringComparison.CurrentCulture) && requestedUri.Length > 0) aResponse.Redirect(aRequest.Uri + "/");

                if (File.Exists(Path.Combine(completePath, "index.html")))
                {
                    completePath = Path.Combine(completePath, "index.html");
                }
                else
                {
                    logger.WriteLine("HERERERERE");
                    RenderIndex(aResponse, completePath);
                    return true;
                }

            }

            if (!File.Exists(completePath))
            {
                NotFound(aResponse);
                return false;
            }
            else
            {
                ServeFile(aResponse, completePath);
                return true;
            }


        }

        public void ServeFile(IHttpResponse aResponse, string aPath)
        {

            var fileRS = File.OpenRead(aPath);


            aResponse.ContentType = MimeHelper.GetMimeFromExtension(Path.GetExtension(aPath));

            fileRS.CopyTo(aResponse.Body);


        }

        public void NotFound(IHttpResponse aResponse)
        {
            aResponse.Status = System.Net.HttpStatusCode.NotFound;
            var writer = new StreamWriter(aResponse.Body);
            writer.WriteLine("Error 404 - File Not Found");
            writer.Flush();


        }

        public void MethodNotAllowed(IHttpResponse aResponse)
        {
            aResponse.Status = System.Net.HttpStatusCode.MethodNotAllowed;
            var writer = new StreamWriter(aResponse.Body);

            writer.WriteLine("Error 405 - Method Not Allowed");
            writer.Flush();

        }

        public void RenderIndex(IHttpResponse aResponse, string aPath)
        {

            var template = GetResourceFileContentAsString("template.html");

            var filesAndDirs = DirSearch(aPath);
            string htmlList = "";

            foreach (var node in filesAndDirs)
            {
                var pathname = Path.GetFileName(node);
                htmlList += $"<li><a href='./{pathname}'>{pathname}</a></li>";
            }

            htmlList += "<li><a href='../'>..</a></li>";

            var result = template.Replace("{LIST}", htmlList);
            result = result.Replace("{DIRNAME}", Path.GetDirectoryName(aPath));


            var writer = new StreamWriter(aResponse.Body);
            writer.WriteLine(result);
            writer.Flush();

            //this.Host.Logger.WriteLine();
        }

        public string GetResourceFileContentAsString(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ArchBench.PlugIns.Serve." + fileName;

            string resource = null;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    resource = reader.ReadToEnd();
                }
            }
            return resource;
        }

        private List<String> DirSearch(string sDir)
        {
            List<String> files = new List<String>();

            foreach (string f in Directory.GetFiles(sDir))
            {
                files.Add(f);
            }
            foreach (string d in Directory.GetDirectories(sDir))
            {
                files.Add(d);
            }



            return files;
        }
    }

}
