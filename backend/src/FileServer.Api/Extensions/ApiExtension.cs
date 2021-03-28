using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace FileServer.Api.Extensions
{
  public static class ApiExtension
  {
    public static string GetApiKey(this ClaimsPrincipal principal)
    {
      return principal?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.SerialNumber)?.Value;
    }

    public static TAttribute GetAttribute<TAttribute>(this HttpContext context) where TAttribute : Attribute
    {
      var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
      return endpoint?.Metadata.GetMetadata<TAttribute>();
    }
  }
}