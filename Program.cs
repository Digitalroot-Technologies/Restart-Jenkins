using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;

namespace RestartJenkins
{
  class Program
  {
    static void Main()
    {
      var protectedProcessList = new List<string>
                                 {
                                   "nunit-agent-x86",
                                   "nunit-agent",
                                   "nunit-console-x86",
                                   "nunit-console",
                                   "accurev",
                                   "nuget",
                                   "msbuild",
                                   "cmd",
                                   "ccrewrite"
                                 };

      var processlist = Process.GetProcesses();

      foreach (var theprocess in processlist.Where(theprocess => protectedProcessList.Contains(theprocess.ProcessName.ToLower())))
      {
        Console.WriteLine("{0} is running.", theprocess.ProcessName);
        Environment.Exit(0);
      }

      Console.WriteLine("Restarting Jenkins");
      RestartService("Jenkins", 500);
    }

    /// <summary>
    /// Method taken from http://www.csharp-examples.net/restart-windows-service/
    /// Author: Jan Slama, 08-May-2008
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="timeoutMilliseconds"></param>
    public static void RestartService(string serviceName, int timeoutMilliseconds)
    {
      var service = new ServiceController(serviceName);
      try
      {
        var millisec1 = Environment.TickCount;
        var timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

        service.Stop();
        service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

        // count the rest of the timeout
        var millisec2 = Environment.TickCount;
        timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

        service.Start();
        service.WaitForStatus(ServiceControllerStatus.Running, timeout);
      }
      catch
      {
        
      }
    }
  }
}
