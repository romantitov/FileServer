using Microsoft.Extensions.Configuration;

namespace FileServer.Tests.Specifications.Utils
{
  public static class ConfigurationExtensions
  {
    public static string GetFileServerApiBaseUrl(this IConfiguration configuration)
    {
      return configuration["FileServerApi:BaseUrl"];
    }

    public static string GetFileServerApiKey(this IConfiguration configuration, int keyNumber)
    {
      return configuration[$"FileServerApi:ApiKeys:Key{keyNumber}"];
    }
  }
}
