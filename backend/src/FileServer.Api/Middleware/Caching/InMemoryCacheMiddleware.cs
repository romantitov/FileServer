using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FileServer.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace FileServer.Api.Middleware.Caching
{
  public class InMemoryCacheMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly string _startPrefix;

    public InMemoryCacheMiddleware(RequestDelegate next, string startPrefix)
    {
      _next = next;
      _startPrefix = startPrefix;
    }

    public async Task Invoke(HttpContext context, IMemoryCache memoryCache, IOptions<CacheConfig> options)
    {
      var attribute = context.GetAttribute<NoCacheAttribute>();
      if (options == null) throw new ArgumentNullException(nameof(options));
      if (_startPrefix != null && context.Request.Path.StartsWithSegments(_startPrefix) && attribute == null)
      {
        var key = $"{context.Request.Method}_{context.Request.Path}{context.Request.QueryString}";

        if (context.Request.Method == HttpMethod.Get.Method || context.Request.Method == HttpMethod.Head.Method)
        {
          if (memoryCache.TryGetValue<ResponseObject>(key, out var cachedData))
          {
            foreach (var header in cachedData.Headers)
            {
              context.Response.Headers[header.Key] = header.Value;
            }

            if (cachedData.Body.Length > 0)
            {
              await context.Response.Body.WriteAsync(cachedData.Body);
            }
            return;
          }
        }

        context.Request.EnableBuffering();
        var originalBody = context.Response.Body;
        try
        {
          await using var memoryStream = new MemoryStream();
          context.Response.Body = memoryStream;
          await _next(context);

          var obj = new ResponseObject(context.Response.Headers, memoryStream.ToArray());
          await originalBody.WriteAsync(obj.Body);
          memoryCache.Set(key, obj, DateTimeOffset.UtcNow.AddSeconds(options.Value.ResponsesTtlSecs));
        }
        finally
        {
          context.Response.Body = originalBody;
        }

      }
      else
      {
        await _next(context);
      }

    }

    private class ResponseObject
    {
      public ResponseObject(IHeaderDictionary headers, byte[] body)
      {
        Headers = new Dictionary<string, StringValues>(headers);

        Body = body;
      }

      public Dictionary<string, StringValues> Headers { get; }
      public byte[] Body { get; }
    }
  }
}
