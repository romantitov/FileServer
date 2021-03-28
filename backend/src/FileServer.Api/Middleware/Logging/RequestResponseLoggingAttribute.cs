using System;

namespace FileServer.Api.Middleware.Logging
{
  [AttributeUsage(AttributeTargets.Method)]
  public class RequestResponseLoggingAttribute:Attribute
  {
    public LoggingTypes LoggingTypes { get; }

    public RequestResponseLoggingAttribute(LoggingTypes loggingTypes = LoggingTypes.All)
    {
      LoggingTypes = loggingTypes;
    }
  }
}