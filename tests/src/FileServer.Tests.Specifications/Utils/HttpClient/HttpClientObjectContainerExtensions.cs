using System;
using System.Net.Http;
using BoDi;
using Refit;

namespace FileServer.Tests.Specifications.Utils.HttpClient
{
  public static class HttpClientObjectContainerExtensions
  {
    private static readonly HttpClientHandler HttpClientHandler = new HttpClientHandler { AllowAutoRedirect = false };

    public static void AddRefitClient<T>(this IObjectContainer container, Action<System.Net.Http.HttpClient> action, RefitSettings refitSettings = null)
    {
      var handler = new LoggingHandler(new CorrelationIdHttpClientHandler(HttpClientHandler, container.Resolve<ICorrelationContextAccessor>()), container.Resolve<ILogger>());

      var httpClient = new System.Net.Http.HttpClient(handler)
      {
        Timeout = TimeSpan.FromMinutes(3)
      };
      action(httpClient);
      var client = RestService.For<T>(httpClient, refitSettings);
      container.RegisterInstanceAs(client, typeof(T));
    }
  }
}