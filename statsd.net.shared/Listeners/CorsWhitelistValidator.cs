using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Kayak.Http;

namespace statsd.net.shared.Listeners
{
  public class CorsWhitelistValidator : ICorsValidationProvider
  {
    private readonly List<string> _whitelist;
    private readonly string _flashCrossDomainPolicy;
    private readonly string _silverlightCrossDomainPolicy;

    public CorsWhitelistValidator(string[] whitelist)
    {
      _whitelist = new List<string>(whitelist);

      // cache serialised xml for performance
      var xsFlash = new XmlSerializer(typeof(XmlFlashCrossDomainPolicy));
      using (var sw = new System.IO.StringWriter())
      {
        var xmlFlashCrossDomainPolicy = new XmlFlashCrossDomainPolicy()
        {
          AllowAccessFrom = _whitelist.Select(w => new XmlFlashCrossDomainPolicy.XmlAllowAccessFrom() { Domain = w })
            .ToArray()
        };
        xsFlash.Serialize(
          sw,
          xmlFlashCrossDomainPolicy,
          xmlFlashCrossDomainPolicy.Namespaces
        );
        _flashCrossDomainPolicy = sw.ToString();
      }

      var xsSilverlight = new XmlSerializer(typeof(XmlSilverlightCrossDomainPolicy));
      using (var sw = new System.IO.StringWriter())
      {
        var xmlSilverlightCrossDomainPolicy = new XmlSilverlightCrossDomainPolicy()
        {
          CrossDomainAccess = new XmlSilverlightCrossDomainPolicy.XmlCrossDomainAccess
          {
            Policy = new XmlSilverlightCrossDomainPolicy.XmlPolicy
            {
              AllowFrom = new XmlSilverlightCrossDomainPolicy.XmlAllowFrom
              {
                Domains = _whitelist.Select(w => new XmlSilverlightCrossDomainPolicy.XmlDomain { Uri = w }).ToArray(),
                HttpRequestHeaders = "SOAPAction"
              },
              GrantTo = new XmlSilverlightCrossDomainPolicy.XmlGrantTo
              {
                Resource = new XmlSilverlightCrossDomainPolicy.XmlResource
                {
                  IncludeSubpaths = true,
                  Path = "/"
                }
              }
            }
          }
        };
        xsSilverlight.Serialize(
          sw,
          xmlSilverlightCrossDomainPolicy,
          xmlSilverlightCrossDomainPolicy.Namespaces
        );
        _silverlightCrossDomainPolicy = sw.ToString();
      }
    }

    // Note: Implemented per https://www.w3.org/TR/cors
    // Simple Request (6.1) and Preflight (6.2) have been merged for simplicity
    public Dictionary<string, string> AppendCorsHeaderDictionary(HttpRequestHead head, Dictionary<string, string> headers)
    {
      var result = new Dictionary<string, string>(headers);
      // Prevent caching: removing this will cause upstream cache to return invalid responses to client requests
      result.Add("Vary", "Origin");

      // Verify origin is specified
      if (!head.Headers.ContainsKey("Origin"))
        goto Exit;
      if (head.Headers["Origin"] == "null")
        goto Exit;

      // Match origin to whitelist
      if (!_whitelist.Any(w => string.Equals(w, head.Headers["Origin"], StringComparison.Ordinal)))
        goto Exit;

      // Preflight
      if (head.Method == "OPTIONS")
      {
        // Parse request methods
        var reqMethod = head.Headers.ContainsKey("Access-Control-Request-Method")
          ? head.Headers["Access-Control-Request-Method"]
          : null;
        if (reqMethod == null)
          goto Exit; // Parse failed

        // Parse request headers
        var reqHeaders = head.Headers.ContainsKey("Access-Control-Request-Headers")
          ? head.Headers["Access-Control-Request-Headers"]
            .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
          : null;
        // Not sure that this is required - seems to cause issues with some clients
        //if (reqHeaders == null)
        //  goto Exit; // Parse failed

        // Match all Methods (https://www.w3.org/TR/cors 6.2.5)

        // Match all Headers (https://www.w3.org/TR/cors 6.2.6)

        // Append allowed methods
        result.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");

        // Append allowed headers
        result.Add("Access-Control-Allow-Headers", "X-Requested-With,Content-Type");
      }

      // Append allow
      result.Add("Access-Control-Allow-Origin", head.Headers["Origin"]);
      // Credentials not used
      // Exposed headers not used

      Exit:
      return result;
    }

    public string GetFlashCrossDomainPolicy()
    {
      return _flashCrossDomainPolicy;
    }

    public string GetSilverlightCrossDomainPolicy()
    {
      return _silverlightCrossDomainPolicy;
    }
  }


  [Serializable]
  [DesignerCategory("code")]
  [XmlType(AnonymousType = true)]
  [XmlRoot("cross-domain-policy", Namespace = "urn:FlashCrossDomainPolicy", IsNullable = false)]
  public class XmlFlashCrossDomainPolicy
  {
    private readonly XmlSerializerNamespaces _namespaces;

    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces Namespaces => this._namespaces;

    public XmlFlashCrossDomainPolicy()
    {
      this._namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[] {
        new XmlQualifiedName(string.Empty, "urn:FlashCrossDomainPolicy") // Default Namespace
      });
    }

    private XmlAllowAccessFrom[] _allowAccessFrom;

    [XmlElement("allow-access-from")]
    public XmlAllowAccessFrom[] AllowAccessFrom
    {
      get => _allowAccessFrom;
      set => _allowAccessFrom = value;
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlAllowAccessFrom
    {

      private string _domain;

      [XmlAttribute("domain")]
      public string Domain
      {
        get => _domain;
        set => _domain = value;
      }
    }
  }



  [Serializable]
  [DesignerCategory("code")]
  [XmlType(AnonymousType = true)]
  [XmlRoot("access-policy", Namespace = "urn:SilverlightCrossDomainPolicy", IsNullable = false)]
  public class XmlSilverlightCrossDomainPolicy
  {
    private readonly XmlSerializerNamespaces _namespaces;

    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces Namespaces => this._namespaces;

    public XmlSilverlightCrossDomainPolicy()
    {
      this._namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[] {
        new XmlQualifiedName(string.Empty, "urn:SilverlightCrossDomainPolicy") // Default Namespace
      });
    }

    private XmlCrossDomainAccess _crossDomainAccess;

    [XmlElement("cross-domain-access")]
    public XmlCrossDomainAccess CrossDomainAccess
    {
      get => _crossDomainAccess;
      set => _crossDomainAccess = value;
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlCrossDomainAccess
    {

      private XmlPolicy _policy;

      [XmlElement("policy")]
      public XmlPolicy Policy
      {
        get => _policy;
        set => _policy = value;
      }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlPolicy
    {

      private XmlAllowFrom _allowfrom;

      private XmlGrantTo _grantTo;

      [XmlElement("allow-from")]
      public XmlAllowFrom AllowFrom
      {
        get => _allowfrom;
        set => _allowfrom = value;
      }

      [XmlElement("grant-to")]
      public XmlGrantTo GrantTo
      {
        get => _grantTo;
        set => _grantTo = value;
      }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlAllowFrom
    {

      private XmlDomain[] _domains;

      private string _httpRequestHeaders;

      [XmlElement("domain")]
      public XmlDomain[] Domains
      {
        get => _domains;
        set => _domains = value;
      }

      [XmlAttribute("http-request-headers")]
      public string HttpRequestHeaders
      {
        get => _httpRequestHeaders;
        set => _httpRequestHeaders = value;
      }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlDomain
    {

      private string _uri;

      [XmlAttribute("uri")]
      public string Uri
      {
        get => _uri;
        set => _uri = value;
      }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlGrantTo
    {

      private XmlResource _resource;

      [XmlElement("resource")]
      public XmlResource Resource
      {
        get => _resource;
        set => _resource = value;
      }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlResource
    {

      private bool _includeSubpaths;

      private string _path;

      [XmlAttribute("include-subpaths")]
      public bool IncludeSubpaths
      {
        get => _includeSubpaths;
        set => _includeSubpaths = value;
      }

      [XmlAttribute("path")]
      public string Path
      {
        get => _path;
        set => _path = value;
      }
    }
  }
}