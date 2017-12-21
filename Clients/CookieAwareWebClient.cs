using System;
using System.Net;

namespace RestartJenkins.Clients
{
  /// <summary>
  /// This is a Cookie Aware version of the WebClient class.
  /// 
  /// Also Added a few methods for doing Post and Get Requests.
  /// </summary>
  public class CookieAwareWebClient : WebClient
  {
    /// <summary>
    /// Used to enable Cookies for this web client.
    /// </summary>
    internal CookieContainer CookieContainer = new CookieContainer();

    /// <summary>
    /// Returns a <see cref="T:System.Net.WebRequest"/> object for the specified resource.
    /// </summary>
    /// <returns>
    /// A new <see cref="T:System.Net.WebRequest"/> object for the specified resource.
    /// </returns>
    /// <param name="address">A <see cref="T:System.Uri"/> that identifies the resource to request.</param>
    protected override WebRequest GetWebRequest(Uri address)
    {
      WebRequest request = base.GetWebRequest(address);
      if (request is HttpWebRequest)
      {
        (request as HttpWebRequest).CookieContainer = CookieContainer;
      }
      return request;
    }
  }
}
