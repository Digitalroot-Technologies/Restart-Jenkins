namespace RestartJenkins.Protocol
{
  public class Crumb
  {
    public string crumb;
    public string crumbRequestField;

    public Crumb(string crumb, string crumbRequestField)
    {
      this.crumb = crumb;
      this.crumbRequestField = crumbRequestField;
    }
  }
}
