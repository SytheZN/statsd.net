using System.Collections.Generic;
using Kayak.Http;

namespace statsd.net.shared.Listeners
{
  public class CorsStarValidator : ICorsValidationProvider
  {
    private const string FLASH_CROSSDOMAIN = "<?xml version=\"1.0\" ?>\r\n<cross-domain-policy>\r\n  <allow-access-from domain=\"*\" />\r\n</cross-domain-policy>\r\n";
    private const string SILVERLIGHT_CROSSDOMAIN = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n <access-policy>\r\n <cross-domain-access>\r\n <policy>\r\n <allow-from http-request-headers=\"SOAPAction\">\r\n <domain uri=\"http://*\"/>\r\n <domain uri=\"https://*\" />\r\n </allow-from>\r\n <grant-to>\r\n <resource include-subpaths=\"true\" path=\"/\"/>\r\n </grant-to>\r\n </policy>\r\n </cross-domain-access>\r\n </access-policy>\r\n";

    public CorsStarValidator()
    {

    }

    public Dictionary<string, string> AppendCorsHeaderDictionary(HttpRequestHead head, Dictionary<string, string> headers)
    {
      if (head.Method == "OPTIONS")
        return new Dictionary<string, string>(headers)
        {
          {"Access-Control-Allow-Origin", "*"},
          {"Access-Control-Allow-Methods", "GET, POST, OPTIONS"},
          {"Access-Control-Allow-Headers", "X-Requested-With,Content-Type"}
        };

      return new Dictionary<string, string>(headers)
      {
        {"Access-Control-Allow-Origin", "*"}
      };
    }

    public string GetFlashCrossDomainPolicy()
    {
      return FLASH_CROSSDOMAIN;
    }

    public string GetSilverlightCrossDomainPolicy()
    {
      return SILVERLIGHT_CROSSDOMAIN;
    }
  }
}