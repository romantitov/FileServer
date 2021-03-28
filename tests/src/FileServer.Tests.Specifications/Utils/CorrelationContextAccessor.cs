namespace FileServer.Tests.Specifications.Utils
{
  public interface ICorrelationContextAccessor
  {
    string CorrelationId { get; }
    string Header { get; }
  }
  public class CorrelationContextAccessor : ICorrelationContextAccessor
  {
    public CorrelationContextAccessor(string correlationId, string header = null)
    {
      CorrelationId = correlationId;
      Header = header ?? "X-Correlation-ID";
    }

    public string CorrelationId { get; }
    public string Header { get; }
  }
}
