using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HttpServer;

namespace ArchBench.PlugIns.Broker
{
    public class Form
    {
        static public bool IsUrlEncoded(string aContentType)
        {
            if (aContentType == null) return false;
            return aContentType.IndexOf("application/x-www-form-urlencoded", StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        static public bool IsMultipart(string aContentType)
        {
            if (aContentType == null) return false;
            return aContentType.IndexOf("multipart/form-data", StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

       

        static public void SendForm(HttpForm aForm, HttpWebRequest aRequest)
        {
            StringBuilder formData = new StringBuilder();

            int i = 0;
            foreach (HttpInputItem input in aForm)
            {
                if (i > 0) formData.Append("&");
                formData.Append($"{input.Name}={Uri.EscapeDataString(input.Value)}");
                i++;
            }

            byte[] postBytes =  Encoding.ASCII.GetBytes(formData.ToString());

            aRequest.ContentLength = postBytes.Length;
            Stream stream = aRequest.GetRequestStream();

            stream.Write(postBytes, 0, postBytes.Length);
            stream.Flush();


        }

        private static string RenameFile(String filePath, String newName)
        {
           var newPath = $"{Path.GetDirectoryName(filePath)}\\{newName}";
           MoveWithReplace(filePath, newPath);
           return newPath;
        }

        private static void MoveWithReplace(string sourceFileName, string destFileName)
        {

            //first, delete target file if exists, as File.Move() does not support overwrite
            if (File.Exists(destFileName))
            {
                File.Delete(destFileName);
            }

            File.Move(sourceFileName, destFileName);

        }

        public static void SendMultipart( HttpForm aForm ,HttpWebRequest aRequest)
        {
            // string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            aRequest.ServicePoint.Expect100Continue = false;

            string boundary = Guid.NewGuid().ToString();


            aRequest.ContentType = $"multipart/form-data; boundary={boundary}";
            aRequest.Method = "POST";
          //  aRequest.KeepAlive = true;

            Stream memStream = new System.IO.MemoryStream();

            var boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            var endBoundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--");
            string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

       
            foreach (HttpInputItem form in aForm)
            {
                string formitem = string.Format(formdataTemplate, form.Name, form.Value);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                memStream.Write(formitembytes, 0, formitembytes.Length);
            }
      
            string headerTemplate =
                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                "Content-Type: {2}\r\n\r\n";

            foreach(HttpFile file in aForm.Files)
            {
                var path = RenameFile(file.Filename, file.UploadFilename);
                memStream.Write(boundarybytes, 0, boundarybytes.Length); // Boundary
                var header = string.Format(headerTemplate, file.Name, path, file.ContentType); //Maybe  UploadFileName
                var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                memStream.Write(headerbytes, 0, headerbytes.Length);

                byte[] fileBuffer = File.ReadAllBytes(file.Filename);
                memStream.Write(fileBuffer, 0, fileBuffer.Length);
            }

            memStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            aRequest.ContentLength = memStream.Length;

            using (Stream requestStream = aRequest.GetRequestStream())
            {
                memStream.Position = 0;
                byte[] tempBuffer = new byte[memStream.Length];
                memStream.Read(tempBuffer, 0, tempBuffer.Length);
                memStream.Close();
                requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            }

      
        }

    }
}
