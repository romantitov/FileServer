using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileServer.Tests.Specifications.Utils.HttpClient
{
  public class LoggingHandler : DelegatingHandler
  {
    private readonly ILogger _logger;

    public LoggingHandler(CorrelationIdHttpClientHandler clientHandler, ILogger logger)
      :base(clientHandler)
    {
      _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      var stringBuilder = new StringBuilder();
      stringBuilder.AppendLine($"---------------------------<HTTP>---------------------------");
      var requestBody = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

      stringBuilder.AppendLine($"[ REQUEST ][ {request.Method} ] {request.RequestUri}");
      if (request.Headers != null && request.Headers.Any())
      {
        stringBuilder.AppendLine("[ HEADERS ]");
        foreach (var requestHeader in request.Headers)
        {
          stringBuilder.AppendLine($"\t{requestHeader.Key} : {string.Join(",", requestHeader.Value)}");
        }
      }

      if (!string.IsNullOrEmpty(requestBody))
      {
        stringBuilder.AppendLine($"Request body: {requestBody}");
      }
      try
      {
        var response = await base.SendAsync(request, cancellationToken);

        var responseBody = response.Content == null ? null : await response.Content.ReadAsStringAsync(cancellationToken);

        stringBuilder.AppendLine($"[ RESPONSE ][ {response.RequestMessage.Method} ] [{response.StatusCode}] {response.RequestMessage.RequestUri}");
        if (!string.IsNullOrEmpty(responseBody))
        {
          stringBuilder.AppendLine($"Response body: {responseBody}");
        }
        return response;
      }
      catch (Exception ex)
      {
        stringBuilder.AppendLine($"Error: {ex}");
        throw;
      }
      finally
      {
        _logger.Log(stringBuilder.ToString());
      }

    }
  }
}