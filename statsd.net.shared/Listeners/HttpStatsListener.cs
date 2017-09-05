using Kayak;
using Kayak.Http;
using statsd.net.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web;

namespace statsd.net.shared.Listeners
{
  public class HttpStatsListener : IListener
  {
    private IScheduler _scheduler;
    private ISystemMetricsService _systemMetrics;
    private ITargetBlock<string> _target;
    private int _port;
    private static ICorsValidationProvider _corsValidator;

    public HttpStatsListener(int port, ISystemMetricsService systemMetrics, ICorsValidationProvider corsValidator)
    {
      _systemMetrics = systemMetrics;
      _port = port;
      _corsValidator = corsValidator;
    }

    public void LinkTo(ITargetBlock<string> target, CancellationToken token)
    {
      _target = target;
      IsListening = true;
      _scheduler = KayakScheduler.Factory.Create(new SchedulerDelegate());
      var wsTask = Task.Factory.StartNew(() =>
      {
        var server = KayakServer.Factory.CreateHttp(
          new RequestDelegate(this),
          _scheduler);

        using (server.Listen(new IPEndPoint(IPAddress.Any, _port)))
        {
          _scheduler.Start();
        }
        IsListening = false;
      });

      Task.Factory.StartNew(() =>
      {
        token.WaitHandle.WaitOne();
        _scheduler.Stop();
      });
    }

    public bool IsListening { get; private set; }

    private class SchedulerDelegate : ISchedulerDelegate
    {
      public void OnException(IScheduler scheduler, Exception e)
      {
        // Ignore
      }

      public void OnStop(IScheduler scheduler)
      {
      }
    }

    private class RequestDelegate : IHttpRequestDelegate
    {
      private HttpStatsListener _parent;
      public RequestDelegate(HttpStatsListener parent)
      {
        _parent = parent;
      }

      public void OnRequest(HttpRequestHead head,
        IDataProducer body,
        IHttpResponseDelegate response)
      {
        if (head.Method.ToUpperInvariant() == "OPTIONS")
        {
          ProcessOPTIONSRequest(head, body, response);
        }
        else if (head.Method.ToUpperInvariant() == "POST")
        {
          ProcessPOSTRequest(head, body, response);
        }
        else if (head.Method.ToUpperInvariant() == "GET" && head.Uri == "/crossdomain.xml")
        {
          ProcessCrossDomainRequest(head, body, response);
        }
        else if (head.Method.ToUpperInvariant() == "GET" && head.Uri == "/clientaccesspolicy.xml")
        {
          ProcessClientAccessPolicyRequest(head, body, response);
        }
        else if (head.Method.ToUpperInvariant() == "GET" && head.QueryString?.Contains("metrics") == true)
        {
          ProcessGETRequest(head, body, response);
        }
        else if (head.Method.ToUpperInvariant() == "GET" && head.Uri == "/")
        {
          ProcessLoadBalancerRequest(head, body, response);
        }
        else
        {
          ProcessFileNotFound(head, body, response);
        }
      }

      private void ProcessOPTIONSRequest(HttpRequestHead head, IDataProducer body, IHttpResponseDelegate response)
      {
        var responseHead = new HttpResponseHead()
        {
          Status = "200 OK",
          Headers = _corsValidator.AppendCorsHeaderDictionary(
            head,
            new Dictionary<string, string>
            {
              {"Content-Type", "text/plain"},
              {"Content-Length", "0"}
            })
        };
        response.OnResponse(responseHead, new EmptyResponse());
      }

      private void ProcessPOSTRequest(HttpRequestHead head, IDataProducer body, IHttpResponseDelegate response)
      {
        body.Connect(new BufferedConsumer(
          (payload) =>
          {
            try
            {
              _parent._systemMetrics.LogCount("listeners.http.bytes", Encoding.UTF8.GetByteCount(payload));
              // Further split by ',' to match the GET while keeping backward compatibility and allowing you to use the join for both methods.
              var lines = payload.Replace("\r", "")
                .Split(new char[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);

              for (var index = 0; index < lines.Length; index++)
              {
                _parent._target.Post(lines[index]);
              }
              _parent._systemMetrics.LogCount("listeners.http.lines", lines.Length);
              Respond(head, response, "200 OK");
            }
            catch
            {
              Respond(head, response, "400 bad request");
            }

          },
          (error) =>
          {
            Respond(head, response, "500 Internal server error");
          }));
      }

      private void ProcessCrossDomainRequest(HttpRequestHead head, IDataProducer body, IHttpResponseDelegate response)
      {
        var flashCrossDomain = _corsValidator.GetFlashCrossDomainPolicy();
        var responseHead = new HttpResponseHead()
        {
          Status = "200 OK",
          Headers = _corsValidator.AppendCorsHeaderDictionary(
            head,
            new Dictionary<string, string>
            {
              { "Content-Type", "application-xml" },
              { "Content-Length", Encoding.UTF8.GetByteCount(flashCrossDomain).ToString() }
            }
           )
        };
        response.OnResponse(responseHead, new BufferedProducer(flashCrossDomain));
      }

      private void ProcessClientAccessPolicyRequest(HttpRequestHead head, IDataProducer body, IHttpResponseDelegate response)
      {
        var silverlightCrossdomain = _corsValidator.GetSilverlightCrossDomainPolicy();
        var responseHead = new HttpResponseHead()
        {
          Status = "200 OK",
          Headers = _corsValidator.AppendCorsHeaderDictionary(
            head,
            new Dictionary<string, string>
            {
              { "Content-Type", "text/xml" },
              { "Content-Length", Encoding.UTF8.GetByteCount(silverlightCrossdomain).ToString() }
            }
          )
        };
        response.OnResponse(responseHead, new BufferedProducer(silverlightCrossdomain));
      }


      private void ProcessGETRequest(HttpRequestHead head, IDataProducer body, IHttpResponseDelegate response)
      {
        var qs = head.QueryString.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
          .Select(p => p.Split(new char[] { '=' }, StringSplitOptions.None))
          .ToDictionary(p => p[0], p => HttpUtility.UrlDecode(p[1]));

        var lines = qs["metrics"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index < lines.Length; index++)
        {
          _parent._target.Post(lines[index]);
        }
        _parent._systemMetrics.LogCount("listeners.http.lines", lines.Length);
        _parent._systemMetrics.LogCount("listeners.http.bytes", Encoding.UTF8.GetByteCount(qs["metrics"]));

        var responseHead = new HttpResponseHead()
        {
          Status = "200 OK",
          Headers = _corsValidator.AppendCorsHeaderDictionary(
            head, 
            new Dictionary<string, string>
            {
              { "Content-Type", "application-xml" },
              { "Content-Length", "0" }
            }
          )
        };
        response.OnResponse(responseHead, new EmptyResponse());
      }

      private void ProcessLoadBalancerRequest(HttpRequestHead head, IDataProducer body, IHttpResponseDelegate response)
      {
        _parent._systemMetrics.LogCount("listeners.http.loadbalancer");
        Respond(head, response, "200 OK");
      }

      private void ProcessFileNotFound(HttpRequestHead head, IDataProducer body, IHttpResponseDelegate response)
      {
        _parent._systemMetrics.LogCount("listeners.http.404");
        var headers = new HttpResponseHead()
        {
          Status = "404 Not Found",
          Headers = _corsValidator.AppendCorsHeaderDictionary(
            head,
            new Dictionary<string, string>
            {
              { "Content-Type", "text/plain" },
              { "Content-Length", Encoding.UTF8.GetByteCount("not found").ToString() }
            }
          )
        };
        response.OnResponse(headers, new BufferedProducer("not found"));
      }

      private void Respond(HttpRequestHead head, IHttpResponseDelegate response, string status)
      {
        var responseHead = new HttpResponseHead()
        {
          Status = status,
          Headers = _corsValidator.AppendCorsHeaderDictionary(
            head,
            new Dictionary<string, string>()
            {
                { "Content-Type", "text/plain" },
                { "Content-Length", "0" }
            }
          )
        };
        response.OnResponse(responseHead, new EmptyResponse());
      }
    }

    private class BufferedConsumer : IDataConsumer
    {
      private readonly List<ArraySegment<byte>> _buffer = new List<ArraySegment<byte>>();
      private readonly Action<string> _callback;
      private readonly Action<Exception> _error;

      public BufferedConsumer(Action<string> callback,
        Action<Exception> error)
      {
        _callback = callback;
        _error = error;
      }

      public bool OnData(ArraySegment<byte> data, Action continuation)
      {
        _buffer.Add(data);
        return false;
      }

      public void OnEnd()
      {
        var payload = _buffer
          .Select(p => Encoding.UTF8.GetString(p.Array, p.Offset, p.Count))
          .Aggregate((result, next) => result + next);
        _callback(payload);
      }

      public void OnError(Exception e)
      {
        _error(e);
      }
    }

    private class BufferedProducer : IDataProducer
    {
      private readonly ArraySegment<byte> _rawData;

      public BufferedProducer(string data)
      {
        _rawData = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));
      }

      public IDisposable Connect(IDataConsumer channel)
      {
        channel.OnData(_rawData, null);
        channel.OnEnd();
        return null;
      }
    }

    private class EmptyResponse : IDataProducer
    {
      public EmptyResponse()
      {
      }

      public IDisposable Connect(IDataConsumer channel)
      {
        channel.OnData(new ArraySegment<byte>(), null);
        channel.OnEnd();
        return null;
      }
    }
  }
}
