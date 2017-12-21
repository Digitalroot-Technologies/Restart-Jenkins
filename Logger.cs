namespace RestartJenkins
{
  class Logger
  {
    public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
  }
}
