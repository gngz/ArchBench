using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
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

        static public void SendMultipart(HttpForm aForm, HttpWebRequest aRequest)
        {
            MultipartFormDataContent form = new MultipartFormDataContent();
            foreach (HttpInputItem item in aForm)
            {
                form.Add(new StringContent(item.Value), item.Name);

            }

            foreach (var file in aForm.Files)
            {
                byte[] buf = File.ReadAllBytes(file.Filename);
                form.Add(new ByteArrayContent(buf, 0, buf.Length), file.Name, file.Filename);
            }

            aRequest.ContentType = form.Headers.ContentType.ToString();

            Stream stream = aRequest.GetRequestStream();

            form.CopyToAsync(stream).Wait();
        }

        static public void SendForm(HttpForm aForm, HttpWebRequest aRequest)
        {

            // use ToString();
          /*
            StringBuilder formData = new StringBuilder();

            int i = 0;
            foreach (HttpInputItem input in aForm)
            {
                if (i > 0) formData.Append("&");
                formData.Append($"{input.Name}={Uri.EscapeDataString(input.Value)}");
                i++;
            }*/

            byte[] postBytes =  Encoding.ASCII.GetBytes(aForm.ToString());

            aRequest.ContentLength = postBytes.Length;
            Stream stream = aRequest.GetRequestStream();

            stream.Write(postBytes, 0, postBytes.Length);
            stream.Flush();


        }

    }
}
