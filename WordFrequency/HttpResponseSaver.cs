using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WordFrequency
{
    class HttpResponseSaver
    {
        public async static Task SaveToFile(string url, string destination, int bufferSize)
        {
            try
            {
                string content = string.Empty;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Credentials = CredentialCache.DefaultCredentials;
                WebResponse response = await request.GetResponseAsync();

                using (var fs = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.GetResponseStream().CopyToAsync(fs, bufferSize);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}
