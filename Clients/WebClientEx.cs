using System;
using System.Net;

namespace RestartJenkins.Clients
{
  class WebClientEx : WebClient
  {
    // https://stackoverflow.com/questions/6726889/how-do-you-prevent-the-webclient-class-from-automatically-following-the-location
    protected override WebRequest GetWebRequest(Uri address)
    {
      var request = (HttpWebRequest)base.GetWebRequest(address);
      request.AllowAutoRedirect = false;
      return request;
    }
  }
}
