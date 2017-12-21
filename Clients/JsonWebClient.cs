using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using RestartJenkins.Protocol;

namespace RestartJenkins.Clients
{
  /// <summary>
  /// This is a version of the .Net WebClient that is Cookie Aware 
  /// </summary>
  public class JsonWebClient : CookieAwareWebClient
  {
    private readonly string _cookiePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                          Path.DirectorySeparatorChar + "DigitalrootJsonWebClient";

    private readonly string _cookieFile;

    public HttpStatusCode RequestStatusCode { get; private set; }

    public JsonWebClient()
    {
      _cookieFile = _cookiePath + Path.DirectorySeparatorChar + "JsonWebClient.dat";
      // Adds Headers to the WebClient used to Query the Server.
      Headers.Add("user-agent", "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.125 Safari/533.4 DSR/1.4.0");
      ServicePointManager.Expect100Continue = false; // Stops the web client from sending HTTP 100 requests.

      if (File.Exists(_cookieFile))
      {
        // Load Cookies
        using (var reader = new StreamReader(_cookieFile))
        {
          while (!reader.EndOfStream)
          {
            var cookie = JsonConvert.DeserializeObject<Cookie>(reader.ReadLine());
            if (cookie != null)
            {
              CookieContainer.Add(cookie);
            }
          }
          reader.Close();
        }
      }
    }

    ~JsonWebClient()
    {
      // Save Cookies
      CookieCollection cookies = CookieContainer.GetCookies(new Uri("http://www.google.com"));

      if (!Directory.Exists(_cookiePath))
      {
        Directory.CreateDirectory(_cookiePath);
      }

      using (var writer = new StreamWriter(_cookieFile, false))
      {
        foreach (Cookie cookie in cookies)
        {
          writer.WriteLine(JsonConvert.SerializeObject(cookie));
        }
        writer.Close();
      }
    }

    /// <summary>
    /// Converts from byte[] to string.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static string Decode(byte[] input)
    {
      return Encoding.ASCII.GetString(input);
    }

    /// <summary>
    /// Converts from string to byte[].
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private byte[] Encode(string input)
    {
      return Encoding.ASCII.GetBytes(input);
    }

    /// <summary>
    /// Does an HTTP Get.
    /// </summary>
    /// <param name="uri">http://www.example.com/login.aspx?mail=joe@example.com&password=mypass</param>
    /// <returns>The response from the web server at a string.</returns>
    public string HttpGet(string uri)
    {
      WebClientEx webClient = new WebClientEx();
      string basicAuthToken = Convert.ToBase64String(Encoding.Default.GetBytes($"{((NetworkCredential)Credentials).UserName}:{((NetworkCredential)Credentials).Password}"));
      webClient.Headers["Authorization"] = "Basic " + basicAuthToken;
      return webClient.DownloadString(uri);
    }

    /// <summary>
    /// Does an HTTP Post.
    /// </summary>
    /// <param name="uri">http://www.example.com/login.aspx</param>
    /// <param name="parameters">mail=joe@example.com&password=mypass</param>
    /// <returns>The response from the web server at a string.</returns>
    public string HttpPost(string uri, string parameters)
    {
      WebClientEx webClient = new WebClientEx();
      string basicAuthToken = Convert.ToBase64String(Encoding.Default.GetBytes($"{((NetworkCredential)Credentials).UserName}:{((NetworkCredential)Credentials).Password}"));
      webClient.Headers["Authorization"] = "Basic " + basicAuthToken;
      return webClient.UploadString(uri, "POST", String.Empty);
    }

    public string HttpPost(string uri, string parameters, Crumb crumb)
    {
      WebClientEx webClient = new WebClientEx();
      string basicAuthToken = Convert.ToBase64String(Encoding.Default.GetBytes($"{((NetworkCredential)Credentials).UserName}:{((NetworkCredential)Credentials).Password}"));
      webClient.Headers["Authorization"] = "Basic " + basicAuthToken;
      webClient.Headers[crumb.crumbRequestField] = crumb.crumb;
      return webClient.UploadString(uri, "POST", String.Empty);
    }

    /// <summary>
    /// Queries a URL without any parameters.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public string QueryUrl(string url)
    {
      using (Stream stream = OpenRead(url))
      {
        if (stream != null) return new StreamReader(stream).ReadToEnd();
      }
      return null;
    }

    /// <summary>
    /// Queries a URL with headers that make it look like an Ajax request.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public string QueryUrlAsAJAXRequest(string url)
    {
      Headers.Add("X-Requested-With", "XMLHttpRequest");
      return QueryUrl(url);
    }

    /// <summary>
    /// Queries a URL with headers that make it look like an Ajax request.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="referer"></param>
    /// <returns></returns>
    public string QueryUrlAsAJAXRequest(string url, string referer)
    {
      Headers.Add("X-Requested-With", "XMLHttpRequest");
      Headers.Add("Referer", referer);
      return QueryUrl(url);
    }

    /// <summary>
    /// Queries a URL with headers that make it look like an Ajax request.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="formData"></param>
    /// <returns></returns>
    public string QueryUrlAsAJAXRequest(string url, NameValueCollection formData)
    {
      // Add Ajax headers
      Headers.Add("X-Requested-With", "XMLHttpRequest");
      Headers.Add("Cache-Control", "no-cache");
      Headers.Add("Pragma", "no-cache");
      Headers.Add("X-Qooxdoo-Response-Type", "application/json");

      byte[] responseBytes = UploadValues(url, "POST", formData);
      return Decode(responseBytes);
    }

    /// <summary>
    /// Queries a URL with headers that make it look like a JSON request.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    /// <returns></returns>
    public string QueryUrlAsJSONRequest(string url, JsonRequest json)
    {
      return QueryUrlAsJSONRequest(url, json, null);
    }

    /// <summary>
    /// Queries a URL with headers that make it look like a JSON request.
    /// </summary>
    /// <param name="url">URL to send the request to.</param>
    /// <param name="json">JSON data to send</param>
    /// <param name="referrer">URL of referrer</param>
    /// <returns></returns>
    public string QueryUrlAsJSONRequest(string url, JsonRequest json, string referrer)
    {
      var webRequest = (HttpWebRequest)WebRequest.Create(url);

      // Add Ajax headers - 
      webRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
      webRequest.ContentType = "application/json; charset=utf-8";
      webRequest.Headers.Add("Cache-Control", "no-cache");
      webRequest.Headers.Add("Pragma", "no-cache");
      webRequest.Headers.Add("X-Qooxdoo-Response-Type", "application/json");
      webRequest.Method = "POST";
      webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";

      if (!string.IsNullOrEmpty(referrer))
      {
        webRequest.Referer = referrer;
      }

      webRequest.CookieContainer = CookieContainer;

      Debug("JsonWebClient.QueryUrlAsJSONRequest", "JsonRequest", json.ToString());
      try
      {
        var writer = new StreamWriter(webRequest.GetRequestStream()); // Note: <- Times out here. 
        writer.Write(json.ToString());
        writer.Close();
      }
      catch (WebException e)
      {
        Debug("JsonWebClient.QueryUrlAsJSONRequest", "WebException", e.Message);
        return "";
      }

      try
      {
        var response = (HttpWebResponse)webRequest.GetResponse();
        RequestStatusCode = response.StatusCode;

        Stream stream = response.GetResponseStream();

        if (stream != null)
        {
          using (var sr = new StreamReader(stream))
          {
            return sr.ReadToEnd();
          }
        }
      }
      catch (WebException e)
      {
        Debug("JsonWebClient.QueryUrlAsJSONRequest", "WebException", e.Message);
        return "";
      }
      return null;
    }

    /// <summary>
    /// Debug used for debugging. label
    /// </summary>
    /// <param name="method"></param>
    /// <param name="label"></param>
    /// <param name="text"></param>
    [Conditional("DEBUG")]
    private static void Debug(string method, string label, string text)
    {
      System.Diagnostics.Debug.WriteLine("Method: {0}, {1}: {2}", method, label, text);
    }
  }
}
