using Kayak.Http;

namespace statsd.net.shared.Listeners
{
  public class CorsAnyValidator : ICorsValidationProvider
  {
    public CorsAnyValidator()
    {
      
    }

    public bool ValidateRequest(HttpRequestHead head)
    {
      return true;
    }

    public string GetDomain(HttpRequestHead head)
    {
      return head.Headers["origin"];
    }
  }
}