namespace RestartJenkins.Protocol
{
  public class CrumbJsonResponse
  {
    public string crumb { get; set; }
    public string crumbRequestField { get; set; }
    public string _class { get; set; }

    public Crumb toCrumb()
    {
      return new Crumb(crumb, crumbRequestField);
    }
  }
}
