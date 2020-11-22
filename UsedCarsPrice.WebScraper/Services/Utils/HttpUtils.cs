using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace UsedCarsPrice.WebScraper.Services.Utils
{
    public class HttpUtils
    {
        public static string DownloadPage(string url, ref HttpStatusCode statusCode)
        {
            string content;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = false;
            request.Method = "GET";
            request.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);

                    content = reader.ReadToEnd();
                    statusCode = response.StatusCode;
                }
            }
            catch (Exception e)
            {
                Log.Debug(e.Message);
                return null;
            }

            return content;
        }
    }
}
