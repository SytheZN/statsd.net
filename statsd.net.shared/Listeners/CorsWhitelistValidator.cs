using System.Collections.Generic;
using System.Linq;
using Kayak.Http;

namespace statsd.net.shared.Listeners
{
  public class CorsWhitelistValidator : ICorsValidationProvider
  {
    private List<string> _whitelist;
    public CorsWhitelistValidator(string[] whitelist)
    {
      _whitelist = new List<string>(whitelist);
    }

    public bool ValidateRequest(HttpRequestHead head)
    {
      if (!head.Headers.ContainsKey("Origin")) return false;

      var origins = head.Headers["Origin"].Split(' ');
      var valid = origins.Any(o => _whitelist.Any(o.Contains));
      return valid;
    }

    public string GetDomain(HttpRequestHead head)
    {
      var origins = head.Headers["Origin"].Split(' ');
      return _whitelist.First(w => origins.Any(o => o.Contains(w)));
    }

    public string GetCorsAllowOriginHeader(HttpRequestHead head)
    {
      var domain = GetDomain(head);
      return $"http://{domain} https://{domain}";
    }
  }
}