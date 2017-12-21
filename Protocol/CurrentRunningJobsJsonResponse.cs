using System.Collections.Generic;

namespace RestartJenkins.Protocol
{
  public class CurrentRunningJobsJsonResponse : JsonResponse
  {
    public List<Job> jobs { get; set; }
  }

  public class Job
  {
    public string name { get; set; }
    public LastBuild lastBuild { get; set; }

  }

  public class LastBuild  
  {
    public bool building { get; set; }
    public string timestamp { get; set; }
  }
}
