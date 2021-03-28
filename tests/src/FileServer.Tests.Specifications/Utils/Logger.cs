using System;
using System.Diagnostics;

namespace FileServer.Tests.Specifications.Utils
{
  public interface ILogger
  {
    void Log(string message);
  }

  public class Logger : ILogger
  {
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public Logger(ICorrelationContextAccessor correlationContextAccessor)
    {
      _correlationContextAccessor = correlationContextAccessor;
    }
    public void Log(string message)
    {
      Console.WriteLine(string.Empty);
      Debug.WriteLine(string.Empty);
      var title = $"CID: {_correlationContextAccessor.CorrelationId} - {DateTimeOffset.UtcNow}";
      Console.WriteLine(title);
      Debug.WriteLine(title);
      Console.WriteLine(message);
      Debug.WriteLine(message);
    }
  }
}
