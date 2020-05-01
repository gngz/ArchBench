using System;
using System.IO;
using System.Net;
using System.Text;
using HttpServer;

namespace ArchBench.PlugIns.Broker.Utils
{
    public class Form
    {
        static public bool IsFormContentType(string aContentType)
        {
            return aContentType.IndexOf("multipart/form-data", StringComparison.CurrentCultureIgnoreCase) >= 0 || aContentType.IndexOf("application/x-www-form-urlencoded", StringComparison.CurrentCultureIgnoreCase) >= 0;
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
            Stream aStream = aRequest.GetRequestStream();

            aStream.Write(postBytes, 0, postBytes.Length);
            aStream.Flush();


        }

    }
}
