using System;
using System.Collections.Generic;
using RestartJenkins.Clients;
using RestartJenkins.Protocol;

namespace RestartJenkins
{
  class Program
  {
    private static readonly JenkinsApiWebClient jenkinsApiWebclient = JenkinsApiWebClient.Instance;

    static void Main()
    {
      Write("==[ Restart Jenkins Started ]==");

      jenkinsApiWebclient.JenkinApiUrl = System.Configuration.ConfigurationManager.AppSettings.Get("JenkinsApiUrl");
      jenkinsApiWebclient.Port = System.Configuration.ConfigurationManager.AppSettings.Get("JenkinsApiPort");
      jenkinsApiWebclient.SetCredentials(System.Configuration.ConfigurationManager.AppSettings.Get("JenkinsApiUsername"),
                                         System.Configuration.ConfigurationManager.AppSettings.Get("JenkinsApiPassword"));

      var runningJobs = GetRunningJob();

      if (runningJobs.Count > 0)
      {
        foreach (Job job in runningJobs)
        {
          Write($"{job.name} running.");
        }
      }
      else
      {
        Write("No Jobs running.");
        Write("Restarting Jenknins.");
        jenkinsApiWebclient.SafeRestart();
      }

      Write("==[ Restart Jenkins Ending ]==");
#if DEBUG
      Console.ReadKey();
#endif
    }

    private static void Write(string msg)
    {
      Console.WriteLine(msg);
      Logger.Log.Info(msg);
    }

    private static List<Job> GetRunningJob()
    {
      CurrentRunningJobsJsonResponse currentRunningJobsJsonResponse = jenkinsApiWebclient.GetRunningJobs();

      List<Job> list = new List<Job>();
      foreach (var job in currentRunningJobsJsonResponse.jobs)
      {
        if (job.lastBuild == null)
        {
          continue;
        }
        if (job.lastBuild.building)
        {
          list.Add(job);
        }
      }
      return list;
    }
  }
}
