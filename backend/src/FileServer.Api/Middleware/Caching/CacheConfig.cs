namespace FileServer.Api.Middleware.Caching
{
  public class CacheConfig
  {
    /// <summary>
    /// TTL for responses in memory cache
    /// </summary>
    public int ResponsesTtlSecs { get; set; }
  }
}