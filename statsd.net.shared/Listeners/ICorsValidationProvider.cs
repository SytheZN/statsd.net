using System.Web.UI;
using Kayak.Http;

namespace statsd.net.shared.Listeners
{
  public interface ICorsValidationProvider
  {
    bool ValidateRequest(HttpRequestHead head);
    string GetDomain(HttpRequestHead head);
    string GetCorsAllowOriginHeader(HttpRequestHead head);
  }
}