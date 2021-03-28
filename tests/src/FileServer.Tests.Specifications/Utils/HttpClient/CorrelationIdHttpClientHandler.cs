using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FileServer.Tests.Specifications.Utils.HttpClient
{
  public class CorrelationIdHttpClientHandler : DelegatingHandler
  {
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public CorrelationIdHttpClientHandler(
      HttpMessageHandler httpMessageHandler,
      ICorrelationContextAccessor correlationContextAccessor):base(httpMessageHandler)
    {
      _correlationContextAccessor = correlationContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      if (!request.Headers.Contains(this._correlationContextAccessor.Header))
        request.Headers.Add(this._correlationContextAccessor.Header, this._correlationContextAccessor.CorrelationId);
      return base.SendAsync(request, cancellationToken);
    }
  }
}