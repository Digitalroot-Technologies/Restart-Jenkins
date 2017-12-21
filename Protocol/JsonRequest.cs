using Newtonsoft.Json;

namespace RestartJenkins.Protocol
{
  public class JsonRequest : IJsonRequest
  {
    private string _value;

    public object Value
    {
      get
      {
        return _value;
      }
      set
      {
        _value = JsonConvert.SerializeObject(value);
      }
    }

    public override string ToString()
    {
      return _value;
    }

    public JsonRequest() { }

    public JsonRequest(object request)
    {
      Value = request;
    }

    public void SetValueBypassSerializeObject(string newvalue)
    {
      _value = newvalue;
    }
  }
}
