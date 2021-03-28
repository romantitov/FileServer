using System;

namespace FileServer.Api.Middleware.Caching
{
  /// <summary>
  /// The attribute indicates that response of marked method should not be cached
  /// </summary>
  [AttributeUsage(AttributeTargets.Method)]
  public class NoCacheAttribute:Attribute
  {

  }
}