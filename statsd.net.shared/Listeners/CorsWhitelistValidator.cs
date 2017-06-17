using Kayak.Http;

namespace statsd.net.shared.Listeners
{
  public class CorsWhitelistValidator : ICorsValidationProvider
  {
    public CorsWhitelistValidator(string[] whitelist)
    {
      
    }

    public bool ValidateRequest(HttpRequestHead head)
    {
      throw new System.NotImplementedException();
    }

    public string GetDomain(HttpRequestHead head)
    {
      throw new System.NotImplementedException();
    }
  }
}