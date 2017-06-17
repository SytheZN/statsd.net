using Kayak.Http;

namespace statsd.net.shared.Listeners
{
  public class CorsStarValidator : ICorsValidationProvider
  {
    public CorsStarValidator()
    {
      
    }

    public bool ValidateRequest(HttpRequestHead head)
    {
      return true;
    }

    public string GetDomain(HttpRequestHead head)
    {
      return "*";
    }
  }
}