using System;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using RestartJenkins.Protocol;

namespace RestartJenkins.Clients
{
  class JenkinsApiWebClient
  {
    private static JenkinsApiWebClient _instance;
    private static readonly object SyncRoot = new object();
    private readonly JsonWebClient _jsonWebClient;
    public string Port { get; set; }
    public string JenkinApiUrl { get; set; }

    private JenkinsApiWebClient()
    {
      _jsonWebClient = new JsonWebClient();
    }

    /// <summary>
    /// Singleton access. 
    /// </summary>
    public static JenkinsApiWebClient Instance
    {
      get
      {
        if (_instance == null)
        {
          lock (SyncRoot)
          {
            if (_instance == null)
              _instance = new JenkinsApiWebClient();
          }
        }
        return _instance;
      }
    }

    public void SetCredentials(string username, string password)
    {
      _jsonWebClient.Credentials = new NetworkCredential(username, password);
    }

    private string HttpGet(string url)
    {
      try
      {
        return _jsonWebClient.HttpGet(url);
      }
      catch (Exception e)
      {
        Logger.Log.Error(e.Message);
        Debug.WriteLine(e.Message);
        throw;
      }
    }

    private string HttpPost(string url, string parameters)
    {
      try
      {
        return _jsonWebClient.HttpPost(url, parameters);
      }
      catch (Exception e)
      {
        Logger.Log.Error(e.Message);
        Debug.WriteLine(e.Message);
        throw;
      }
    }

    private string HttpPost(string url, string parameters, Crumb crumb)
    {
      try
      {
        return _jsonWebClient.HttpPost(url, parameters, crumb);
      }
      catch (Exception e)
      {
        Logger.Log.Error(e.Message);
        Debug.WriteLine(e.Message);
        throw;
      }
    }

    public CurrentRunningJobsJsonResponse GetRunningJobs()
    {
      return JsonConvert.DeserializeObject<CurrentRunningJobsJsonResponse>(HttpGet($"{JenkinApiUrl}:{Port}/api/json?tree=jobs[name,lastBuild[building,timestamp]]"));
    }

    private CrumbJsonResponse GetCrumb()
    {
      return JsonConvert.DeserializeObject<CrumbJsonResponse>(HttpGet($"{JenkinApiUrl}:{Port}/crumbIssuer/api/json"));
    }

    public void SafeRestart()
    {
      HttpPost($"{JenkinApiUrl}:{Port}/safeRestart", null, GetCrumb().toCrumb());
    }
  }
}
