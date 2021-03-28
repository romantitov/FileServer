namespace FileServer.Tests.Specifications.Utils
{
  public static class ScenarioOrders
  {
    public const int Initialize = 0;

    public const int Setup = Initialize + 1;

    public const int Cleanup = Setup + 10;
    public const int Dispose = Cleanup + 1;
    public const int Finalize = Dispose + 1;
  }
}
