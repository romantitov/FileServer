using System;

namespace FileServer.Api.Middleware.Logging
{
  [Flags]
  public enum LoggingTypes
  {
    None = 0b0000,
    Requests = 0b0001,
    Responses = 0b0010,
    All = Requests | Responses
  }
}