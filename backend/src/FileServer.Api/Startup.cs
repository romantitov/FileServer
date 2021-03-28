using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CorrelationId;
using CorrelationId.DependencyInjection;
using FileServer.Api.Middleware;
using FileServer.Api.Middleware.Caching;
using FileServer.Api.Middleware.Logging;
using FileServer.Api.Models;
using FileServer.Api.SwaggerTools;
using FileServer.Binary;
using FileServer.Core;
using FileServer.Core.Exceptions;
using FileServer.MongoDb;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileServer.Api
{
  public class Startup
  {
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
      _configuration = configuration;
    }
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {

      
      services.AddDefaultCorrelationId();
      services.AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Latest)
        .AddFluentValidation(x=>x.RegisterValidatorsFromAssemblyContaining<PageRequestValidator>());
      services.AddControllers();
      services.AddApiVersioning(options =>
      {
        // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
        options.ReportApiVersions = true;
      });
      AddApiKeyAuthentication(services);
     
      AddSwaggerGen(services);
      AddApiVersioning(services);


      services.AddLogging(loggingBuilder =>
      {
        loggingBuilder.AddConsole();
      });
      //Config
      services.Configure<MongoDbConfigurations>(_configuration.GetSection("MongoDb"));
      services.Configure<CacheConfig>(_configuration.GetSection("CacheConfig"));

      var builder = new ContainerBuilder();
      builder.Populate(services);

      builder.RegisterModule<BinaryModule>();
      builder.RegisterModule<CoreModule>();
      builder.RegisterModule<MongoDbModule>();

      return new AutofacServiceProvider(builder.Build());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
    {
     
      app.UseCorrelationId();


      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
          // build a swagger endpoint for each discovered API version
          foreach (var description in provider.ApiVersionDescriptions)
          {
            c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
          }
          c.RoutePrefix = "swagger";
        });
      }

     

      app.UseHttpsRedirection();
      app.UseRouting();
     
      app.UseAuthentication();
      app.UseAuthorization();

      app.UseMiddleware<ErrorHandlingMiddleware>(new Dictionary<Type, HttpStatusCode>
      {
        { typeof(ResourceNotFoundException), HttpStatusCode.NotFound},
        { typeof(AccessNotAllowedException), HttpStatusCode.Forbidden },
        { typeof(ApplicationException), HttpStatusCode.BadRequest },
        { typeof(ValidationException), HttpStatusCode.BadRequest },
      });

      app.UseWhen(context => !RequestResponseLoggingMiddleware.IsGetRequestForSwaggerOrStaticFile(context), appBuilder =>
      {
        appBuilder.UseMiddleware<RequestResponseLoggingMiddleware>(512);
      });
      app.UseMiddleware<InMemoryCacheMiddleware>("/api");
      

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }

    private static void AddApiVersioning(IServiceCollection services)
    {
      services.AddApiVersioning(options =>
      {
        // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
        options.ReportApiVersions = true;
      });

      services.AddVersionedApiExplorer(options =>
      {
        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
        // note: the specified format code will format the version as "'v'major[.minor][-status]"
        options.GroupNameFormat = "'v'VVV";

        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
        // can also be used to control the format of the API version in route templates
        options.SubstituteApiVersionInUrl = true;
      });
    }
    private static void AddSwaggerGen(IServiceCollection services)
    {
      services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
      services.AddSwaggerGen(options =>
      {
        var securityScheme = new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = ApiKeyDefaults.AuthenticationScheme,
          },
          In = ParameterLocation.Header,
          Description = $"Please enter your api key in the field, prefixed with '{ApiKeyDefaults.AuthenticationScheme} '",
          Name = "Authorization"
        };
        options.AddSecurityDefinition(ApiKeyDefaults.AuthenticationScheme, securityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
          {securityScheme, Array.Empty<string>()}
        });

        options.OperationFilter<SwaggerDefaultValues>();


      });
    }

    public void AddApiKeyAuthentication(IServiceCollection services)
    {
      var validApiKeys = _configuration["ApiKeys"].Split(",");
      services.AddAuthentication(options => { options.DefaultScheme = ApiKeyDefaults.AuthenticationScheme; })
        .AddApiKeyInHeaderOrQueryParams(options =>
        {
          options.Realm = "Sample Web API";
          options.KeyName = ApiKeyDefaults.AuthenticationScheme;
          options.Events.OnValidateKey = context =>
          {
            if (validApiKeys.Contains(context.ApiKey))
            {
              context.Principal = new ClaimsPrincipal(new[]
              {
                new ClaimsIdentity(new[]
                {
                  new Claim(ClaimTypes.SerialNumber, context.ApiKey),
                  new Claim(ClaimTypes.Role, "Api")
                }, ApiKeyDefaults.AuthenticationScheme)
              });
              context.Success();
            }
            else
            {
              context.ValidationFailed();
            }
            
            return Task.CompletedTask;
          };
        });
    }

  }
}
