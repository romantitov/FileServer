using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FileServer.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace FileServer.Api.Middleware.Logging
{
  public class RequestResponseLoggingMiddleware
  {
    public readonly int MaximumLogContentSize; // bytes
    public readonly string RequestPrefix = "REQUEST:";
    public readonly string ResponsePrefix = "RESPONSE:";

    private readonly RequestDelegate _next;

    public RequestResponseLoggingMiddleware(RequestDelegate next, int maximumLogContentSize = int.MaxValue)
    {
      _next = next;
      MaximumLogContentSize = maximumLogContentSize;
    }


    public async Task Invoke(HttpContext context, ILogger<RequestResponseLoggingMiddleware> logger)
    {
      var attribute = context.GetAttribute<RequestResponseLoggingAttribute>();
      logger.LogInformation($"EXECUTING: {context.Request.Method} {context.Request.Scheme} {context.Request.Host}{context.Request.Path} {context.Request.QueryString}");
      if (attribute == null || attribute.LoggingTypes == LoggingTypes.None)
      {
        await _next(context);
        return;
      }
      //GetAllMetaData the incoming request
      var requestContent = string.Empty;
      if (attribute.LoggingTypes.HasFlag(LoggingTypes.Requests))
      {
        requestContent = await FormatRequest(context);
      }

      if (attribute.LoggingTypes.HasFlag(LoggingTypes.Responses))
      {
        //Copy a pointer to the original response body stream
        var originalBodyStream = context.Response.Body;

        //Create a new memory stream...
        using (var responseBody = new MemoryStream())
        {
          var response = context.Response;

          //...and use that for the temporary response body
          response.Body = responseBody;

          //Continue down the Middleware pipeline, eventually returning to this class
          await _next(context);

          //Format the response from the server
          var responseContent = await FormatResponse(response);
          if (attribute.LoggingTypes.HasFlag(LoggingTypes.Requests))
          {
            logger.LogInformation(requestContent);
          }
          logger.LogInformation(responseContent);

          //Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
          await responseBody.CopyToAsync(originalBodyStream);
        }
      }
      else
      {
        await _next(context);
        if (attribute.LoggingTypes.HasFlag(LoggingTypes.Requests))
        {
          logger.LogInformation(requestContent);
        }
      }

    }

    public static bool IsGetRequestForSwaggerOrStaticFile(HttpContext context)
    {
      var path = context.Request.Path.Value.TrimStart('/').ToLowerInvariant();

      return context.Request.Method.Equals(HttpMethods.Get, StringComparison.InvariantCultureIgnoreCase)
             && (path.Length == 0 || path.StartsWith("swagger") || path.LastIndexOf('.') > path.LastIndexOf('/'));
    }

    private async Task<string> FormatRequest(HttpContext httpContext)
    {
      var request = httpContext.Request;

      //This line allows us to set the reader for the request back at the beginning of its stream.
      request.EnableBuffering();

      //We now need to read the request stream.  First, we create a new byte[] with the same length as the request stream...
      var buffer = new byte[Math.Min(Convert.ToInt32(request.ContentLength), MaximumLogContentSize)];

      //...Then we copy the entire request stream into the new buffer.
      await request.Body.ReadAsync(buffer, 0, buffer.Length);

      //We convert the byte[] into a string using UTF8 encoding...
      var bodyAsText = Encoding.UTF8.GetString(buffer);

      request.Body.Seek(0, SeekOrigin.Begin);

      return $"{RequestPrefix} \n{bodyAsText}";
    }

    private async Task<string> FormatResponse(HttpResponse response)
    {
      //We need to read the response stream from the beginning...
      response.Body.Seek(0, SeekOrigin.Begin);

      var buffer = new byte[Math.Min(Convert.ToInt32(response.Body.Length), MaximumLogContentSize)];

      await response.Body.ReadAsync(buffer, 0, buffer.Length);

      var bodyText = new StringBuilder(Encoding.UTF8.GetString(buffer));
      if (response.Body.Length > MaximumLogContentSize)
      {
        bodyText.AppendLine(" ...");
        bodyText.AppendLine("Response body too long.");
      }

      //We need to reset the reader for the response so that the client can read it.
      response.Body.Seek(0, SeekOrigin.Begin);

      //Return the string for the response, including the status code (e.g. 200, 404, 401, etc.)
      return $"{ResponsePrefix} {response.StatusCode}: {bodyText}";
    }
  }
}
