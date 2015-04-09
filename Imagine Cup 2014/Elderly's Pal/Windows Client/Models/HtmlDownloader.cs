using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NiceDreamers.Windows.Models
{
    /* Collection of html download functions */
    // =======================================================================
    public class HtmlDownloader
    {
        /* Download html source with WebClient */
        // =======================================================================
        public static String byWebClient(String url, Encoding encoder = null)
        {
            String result = "ERROR!";
            var client = new WebClient();

            if (encoder != null) client.Encoding = encoder;
            else client.Encoding = Encoding.Default;

            try
            {
                result = client.DownloadString(new Uri(url));
            }
            catch (WebException ex)
            {
                return result;
            }

            return result;
        }
        public static string removeHtml(string tmp)
        {
            tmp = WebUtility.HtmlDecode(tmp).Trim();
            tmp = Regex.Replace(tmp, "<!--.*?-->", string.Empty);
            tmp = Regex.Replace(tmp, "<.*?>", string.Empty);

            tmp = tmp.Replace("\t", " ");
            tmp = tmp.Replace("\n", " ");
            while (tmp.Contains("  "))
                tmp = tmp.Replace("  ", " ");
            return tmp.Trim();
        }
        public static async Task<string> loadFromUrl(string url)
        {
            var request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.AllowAutoRedirect = true;
            request.Method = "GET";
            var factory = new TaskFactory();
            var task = factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
            string data = "";
            try
            {
                var response = await task;
                Stream stream = response.GetResponseStream();
                using (var reader = new StreamReader(stream))
                {
                    data = reader.ReadToEnd();
                }
                stream.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
            return data;
        }
        /* Download html source with HttpWebRequest */
        // =======================================================================
        public static String byHttpWebRequest(String url,
            Cookie cookie = null, Encoding encoder = null, int timeOut = 10000)
        {
            String result = "ERROR!";
            if (url.IndexOf("http://", 0) == -1)
                url = "http://" + url;

            // New request
            var request =
                (HttpWebRequest) WebRequest.Create(new Uri(url));
            request.Timeout = timeOut;

            // Cookie goes here!!
            if (cookie != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookie);
            }

            // Encoding style goes here!!
            Encoding encoding;
            if (encoder != null) encoding = encoder;
            else encoding = Encoding.Default;

            // New response
            try
            {
                var response =
                    (HttpWebResponse) request.GetResponse();

                var loResponseStream =
                    new StreamReader(response.GetResponseStream(), encoding);

                result = loResponseStream.ReadToEnd();
                response.Close();
                loResponseStream.Close();
            }
            catch (WebException ex)
            {
                return result;
            }

            return result;
        }
    }

    // =======================================================================
}