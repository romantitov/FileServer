using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FileServer.Api.Middleware
{
  /// <summary>
  /// The middleware to handle exceptions for API calls
  /// </summary>
  public class ErrorHandlingMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly Dictionary<Type, HttpStatusCode> _exceptionCodes;

    /// <summary>
    /// Creates instance of <see cref="ErrorHandlingMiddleware"/>
    /// </summary>
    public ErrorHandlingMiddleware(RequestDelegate next, Dictionary<Type, HttpStatusCode> exceptionCodes)
    {
      _next = next;
      _exceptionCodes = exceptionCodes;
    }

    /// <summary>
    /// Invoke the middleware
    /// </summary>
    public async Task Invoke(HttpContext context, ILogger<ErrorHandlingMiddleware> logger)
    {
      try
      {
        await _next(context);
      }
      catch (Exception ex)
      {
        logger.LogError(ex.ToString());
        HandleExceptionAsync(context, ex);
      }
    }

    private void HandleExceptionAsync(HttpContext context, Exception ex)
    {
      var code = HttpStatusCode.InternalServerError; // 500 if unexpected

      if (_exceptionCodes != null)
      {
        _exceptionCodes.TryGetValue(ex.GetType(), out code);
      }
      else
      {
        if (ex is ApplicationException)
        {
          code = HttpStatusCode.BadRequest;
        }
      }


      context.Response.StatusCode = (int)code;
    }
  }
}
