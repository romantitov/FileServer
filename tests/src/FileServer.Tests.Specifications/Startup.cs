using System;
using System.Net.Http;
using BoDi;
using FileServer.Api.Client;
using FileServer.Tests.Specifications.Utils;
using FileServer.Tests.Specifications.Utils.HttpClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using TechTalk.SpecFlow;


namespace FileServer.Tests.Specifications
{
  [Binding]
  public class Startup
  {
    private static IConfiguration _configuration;
    [BeforeTestRun]
    public static void Configure()
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables();
      _configuration = builder.Build(); 
    }

    [BeforeScenario(Order = ScenarioOrders.Initialize)]
    public static void RegisterServices(IObjectContainer objectContainer)
    {
      objectContainer.RegisterInstanceAs(new CorrelationContextAccessor(Guid.NewGuid().ToString("N")), typeof(ICorrelationContextAccessor));
      objectContainer.RegisterInstanceAs(_configuration, typeof(IConfiguration));
      objectContainer.RegisterTypeAs<Logger, ILogger>();

      //RefitClients
      objectContainer.AddRefitClient<IFilesApiV1>(ConfigureFileServerApiClient());
    }

    private static Action<HttpClient> ConfigureFileServerApiClient()
    {
      return c =>
      {
        var baseUrl = _configuration.GetFileServerApiBaseUrl();
        c.BaseAddress = new Uri(baseUrl);
      };
    }
  }
}
