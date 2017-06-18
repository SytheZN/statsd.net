using System.Collections.Generic;
using System.Web.UI;
using Kayak.Http;

namespace statsd.net.shared.Listeners
{
  public interface ICorsValidationProvider
  {
    Dictionary<string, string> AppendCorsHeaderDictionary(HttpRequestHead head, Dictionary<string, string> headers);
    string GetFlashCrossDomainPolicy();
    string GetSilverlightCrossDomainPolicy();
  }
}