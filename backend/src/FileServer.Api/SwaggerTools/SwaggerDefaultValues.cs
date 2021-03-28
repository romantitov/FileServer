﻿using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileServer.Api.SwaggerTools
{
  public class SwaggerDefaultValues : IOperationFilter
  {
    /// <summary>
    /// Applies the filter to the specified operation using the given context.
    /// </summary>
    /// <param name="operation">The operation to apply the filter to.</param>
    /// <param name="context">The current operation filter context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
      var apiDescription = context.ApiDescription;

      operation.Deprecated |= apiDescription.IsDeprecated();

      if (operation.Parameters == null)
      {
        return;
      }

      foreach (var parameter in operation.Parameters)
      {
        var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
        parameter.Description ??= description.ModelMetadata.Description;
        parameter.Required |= description.IsRequired;
      }
    }
  }
}
