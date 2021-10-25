# WebApi example service

## Req

.NET 5
Docker for Desktop
Git for Windows
PowerShell Core

## Useful links

- [https://github.com/jbogard/MediatR/wiki](https://github.com/jbogard/MediatR/wiki)
- [https://www.c-sharpcorner.com/article/cqrs-mediatr-in-net-5/](https://www.c-sharpcorner.com/article/cqrs-mediatr-in-net-5/)
- [https://jasontaylor.dev/clean-architecture-getting-started/](https://jasontaylor.dev/clean-architecture-getting-started/)

### Example service (SampleService) in clean architecture

```dotnetcli
dotnet new sln --name CompanyName.SampleService
dotnet new classlib --output CompanyName.SampleService.Application
dotnet new classlib --output CompanyName.SampleService.Domain
dotnet new classlib --output CompanyName.SampleService.Infrastructure
dotnet new webapi --output CompanyName.SampleService.WebApi

dotnet sln add ./CompanyName.SampleService.Application/CompanyName.SampleService.Application.csproj
dotnet sln add ./CompanyName.SampleService.Domain/CompanyName.SampleService.Domain.csproj
dotnet sln add ./CompanyName.SampleService.Infrastructure/CompanyName.SampleService.Infrastructure.csproj
dotnet sln add ./CompanyName.SampleService.WebApi/CompanyName.SampleService.WebApi.csproj

cd ./CompanyName.SampleService.WebApi
dotnet add reference ../CompanyName.SampleService.Infrastructure/CompanyName.SampleService.Infrastructure.csproj
cd ../CompanyName.SampleService.Infrastructure
dotnet add reference ../CompanyName.SampleService.Application/CompanyName.SampleService.Application.csproj
cd ../CompanyName.SampleService.Application
dotnet add reference ../CompanyName.SampleService.Domain/CompanyName.SampleService.Domain.csproj
```
### CQRS/MediatR

Add required libraries to the solutions.

Installing *MediatR.Extensions.Microsoft.DependencyInjection* insite *Application* project
```dotnetcli
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
dotnet add package Microsoft.Extensions.Logging.Abstractions
```
####Create *DependencyInjection.cs* file

```csharp
namespace CompanyName.SampleService.Application
{
    using System.Reflection;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
```

Install *Microsoft.Extensions.Configuration.Abstractions* and *Microsoft.Extensions.Configuration.Binder* insite *Infrastructure* project

```dotnetcli
cd ./CompanyName.SampleService.Infrastructure
dotnet add package Microsoft.Extensions.Configuration.Abstractions
dotnet add package Microsoft.Extensions.Configuration.Binder
dotnet add package Microsoft.Extensions.Options.ConfigurationExtensions
```

####Create *DependencyInjection.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure
{
    using System.Reflection;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
```
#### Use *AddInfrastructure* and *AddApplication* in WebApi project.

```csharp
namespace CompanyName.SampleService.WebApi
{
    using System.Reflection;
    using CompanyName.SampleService.Application;
    using CompanyName.SampleService.Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;

    public class Startup
    {
        public Startup(IConfiguration configuration) =>
            this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddApplication();
            services.AddInfrastructure(this.Configuration);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CompanyName.SampleService.WebApi",
                    Version = "v1",
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "CompanyName.SampleService.WebApi v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
```
### Serilog

Add logger infrastructure to the solution

```dotnetcli
dotnet add package Microsoft.IO.RecyclableMemoryStream
dotnet add package Serilog.AspNetCore
```
and now add it to *Program.cs* and *Startup.cs* file

```csharp
namespace CompanyName.SampleService.WebApi
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    public class Program
    {
        private const int EXIT_FAILURE = 1;
        private const int EXIT_SUCCESS = 0;

        public static async Task<int> Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting host");
                await CreateHostBuilder(args).Build().RunAsync();
                return EXIT_SUCCESS;
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Host terminated unexpectedly");
                return EXIT_FAILURE;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
```
finally,  change *appsettings.json* file

```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console"
    ],
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "System": "Information"
      }
    },
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithThreadId"
    ],
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      }
    ]
  }
}
```

### Logging enrichment

#### *SerilogLoggingActionFilter.cs*

```csharp
namespace CompanyName.SampleService.WebApi
{
    using System;
    using System.Diagnostics.Contracts;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Serilog;

    public sealed class SerilogLoggingActionFilter : IActionFilter
    {
        private readonly IDiagnosticContext diagnosticContext;
        public SerilogLoggingActionFilter(IDiagnosticContext diagnosticContext)
        {
            this.diagnosticContext = diagnosticContext
                ?? throw new ArgumentNullException(nameof(diagnosticContext));
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            Contract.Assert(context != null);
            this.diagnosticContext.Set("ActionId", context.ActionDescriptor.Id);
            this.diagnosticContext.Set("ActionName", context.ActionDescriptor.DisplayName);
            this.diagnosticContext.Set("RouteData", context.ActionDescriptor.RouteValues);
            this.diagnosticContext.Set("ValidationState", context.ModelState.IsValid);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
```

#### *RequestResponseLoggingMiddleware.cs*

```csharp
namespace CompanyName.SampleService.WebApi
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;

    internal sealed class RequestResponseLoggingMiddleware
    {
        private readonly ILogger logger;
        private readonly RequestDelegate next;
        private readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<RequestResponseLoggingMiddleware>();
            this.next = next;
            this.recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext context)
        {
            await this.LogRequest(context).ConfigureAwait(true);
            await this.LogResponse(context).ConfigureAwait(true);
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();
            await using var requestStream = this.recyclableMemoryStreamManager.GetStream();
            await context.Request.Body.CopyToAsync(requestStream).ConfigureAwait(true);
            var text = ReadStreamInChunks(requestStream);

            if (!string.IsNullOrEmpty(text))
            {
                this.logger.LogDebug(text);
            }

            context.Request.Body.Position = 0;
        }

        private async Task LogResponse(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            await using var responseBody = this.recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;
            await this.next(context);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            this.logger.LogDebug(text);
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);
            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;

            do
            {
                readChunkLength = reader.ReadBlock(readChunk, 0, readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);

            return textWriter.ToString();
        }
    }

    internal static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
```
and some changes to *Startup.cs* file

```csharp
namespace CompanyName.SampleService.WebApi
{
    using System.Reflection;
    using CompanyName.SampleService.Application;
    using CompanyName.SampleService.Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;
    using Serilog;

    public sealed class Startup
    {
        public Startup(IConfiguration configuration) =>
            this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddSerilog());
            services.AddHealthChecks();

            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddApplication();
            services.AddInfrastructure(this.Configuration);

            services.AddControllers(options =>
            {
                options.Filters.Add<SerilogLoggingActionFilter>();
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CompanyName.SampleService.WebApi",
                    Version = "v1",
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "CompanyName.SampleService.WebApi v1"));
            }

            app.UseHealthChecks("/health");

            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = EnrichDiagnosticContext;
            });

            if (env.IsDevelopment())
            {
                app.UseRequestResponseLogging();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void EnrichDiagnosticContext(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            var request = httpContext.Request;

            diagnosticContext.Set("Host", request.Host);
            diagnosticContext.Set("Protocol", request.Protocol);
            diagnosticContext.Set("Scheme", request.Scheme);

            foreach (var (name, value) in request.Headers)
            {
                diagnosticContext.Set(name, value);
            }

            if (request.QueryString.HasValue)
            {
                diagnosticContext.Set("QueryString", request.QueryString.Value);
            }

            diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

            var endpoint = httpContext.GetEndpoint();

            if (endpoint is { })
            {
                diagnosticContext.Set("EndpointName", endpoint.DisplayName);
            }
        }
    }
}
```
### Add Controller + Query/Query Handler + Service with AutoMapper

Install *AutoMapper.Extensions.Microsoft.DependencyInjection* insite *Infrastructure* project

```dotnetcli
cd ./CompanyName.SampleService.Infrastructure
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

#### Create *CompanyName/SampleService/WebApiControllers/WeatherForecastController.cs* file
```csharp
namespace CompanyName.SampleService.WebApi.Controllers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CompanyName.SampleService.Application.Queries;
    using CompanyName.SampleService.Application.ViewModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("[controller]")]
    public sealed class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> logger;
        private readonly IMediator mediator;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMediator mediator) =>
            (this.logger, this.mediator) = (logger, mediator);

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get(CancellationToken cancellationToken = default) =>
            await this.mediator.Send(new GetWeatherForecasts { }, cancellationToken);
    }
}
```
#### Create *CompanyName/SampleService/Application/Queries/GetWeatherForecasts.cs* file
```csharp
namespace CompanyName.SampleService.Application.Queries
{
    using System.Collections.Generic;
    using CompanyName.SampleService.Application.ViewModels;
    using MediatR;

    public sealed record GetWeatherForecasts : IRequest<IReadOnlyList<WeatherForecast>>
    {
    }
}
```
#### Create *CompanyName/SampleService/Application/ViewModels/WeatherForecast.cs* file
```csharp
namespace CompanyName.SampleService.Application.ViewModels
{
    using System;

    public sealed record WeatherForecast
    {
        public DateTime Date { get; init; }
        public int TemperatureC { get; init; }
        public int TemperatureF { get; init; }
        public string Summary { get; init; }
    }
}
```
#### Create *CompanyName/SampleService/Infrastructure/WeatherForecasts/Interfaces/IWeatherForecastService.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure.WeatherForecasts.Interfaces
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Models;

    internal interface IWeatherForecastService
    {
        Task<IReadOnlyList<WeatherForecast>> Get(CancellationToken cancellationToken = default);
    }
}
```
#### Create *CompanyName/SampleService/Infrastructure/WeatherForecasts/Models/WeatherForecast.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure.WeatherForecasts.Models
{
    using System;

    internal sealed record WeatherForecast
    {
        public DateTime Date { get; init; }
        public int TemperatureC { get; init; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public string Summary { get; init; }
    }
}
```
#### Create *CompanyName/SampleService/Infrastructure/WeatherForecasts/Profiles/WeatherForecastProfile.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure.WeatherForecasts.Profiles
{
    using AutoMapper;
    using SourceWeatherForecast = CompanyName.SampleService.Infrastructure.WeatherForecasts.Models.WeatherForecast;
    using TargetWeatherForecast = CompanyName.SampleService.Application.ViewModels.WeatherForecast;

    internal sealed class WeatherForecastProfile : Profile
    {
        public WeatherForecastProfile()
        {
            this.CreateMap<SourceWeatherForecast, TargetWeatherForecast>()
                .ForMember(target => target.Date, options => options.MapFrom(source => source.Date))
                .ForMember(target => target.Summary, options => options.MapFrom(source => source.Summary))
                .ForMember(target => target.TemperatureC, options => options.MapFrom(source => source.TemperatureC))
                .ForMember(target => target.TemperatureF, options => options.MapFrom(source => source.TemperatureF))
                ;
        }
    }
}
```
More information about how to use AutoMapper you can find on [https://docs.automapper.org/en/latest/Getting-started.html] (https://docs.automapper.org/en/latest/Getting-started.html)



#### Create *CompanyName/SampleService/Infrastructure/WeatherForecasts/QueryHandlers/GetWeatherForecastsHandler.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure.WeatherForecasts.QueryHandlers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CompanyName.SampleService.Application.Queries;
    using CompanyName.SampleService.Application.ViewModels;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Interfaces;
    using AutoMapper;
    using MediatR;

    internal sealed class GetWeatherForecastsHandler : IRequestHandler<GetWeatherForecasts, IReadOnlyList<WeatherForecast>>
    {
        private readonly IMapper mapper;
        private readonly IWeatherForecastService service;

        public GetWeatherForecastsHandler(IMapper mapper, IWeatherForecastService service) =>
            (this.mapper, this.service) = (mapper, service);

        public async Task<IReadOnlyList<WeatherForecast>> Handle(GetWeatherForecasts request, CancellationToken cancellationToken)
        {
            var source = await this.service.Get(cancellationToken);
            var result = this.mapper.Map<IReadOnlyList<WeatherForecast>>(source);
            return result;
        }
    }
}
```
#### Create *CompanyName/SampleService/Infrastructure/WeatherForecasts/Services/WeatherForecastService.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure.WeatherForecasts.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Interfaces;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Models;

    internal sealed class WeatherForecastService : IWeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing",
            "Bracing",
            "Chilly",
            "Cool",
            "Mild",
            "Warm",
            "Balmy",
            "Hot",
            "Sweltering",
            "Scorching",
        };

        public async Task<IReadOnlyList<WeatherForecast>> Get(CancellationToken cancellationToken = default)
        {
            var rng = new Random();
            var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToList();

            return await Task.FromResult(result.AsReadOnly());
        }
    }
}
```

#### Change *CompanyName/SampleService/Infrastructure/DependencyInjection.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure
{
    using System.Reflection;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Interfaces;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Services;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddSingleton<IWeatherForecastService, WeatherForecastService>();

            return services;
        }
    }
}
```
### Connection between services

From the server site use Commands/Queries/ViewModels from Application project
```
CompanyName.SampleChildService.Application.Commands
CompanyName.SampleChildService.Application.Queries
CompanyName.SampleChildService.Application.ViewModels
```
on client use models in Infrastructure project

```
CompanyName.SampleService.Infrastructure.SampleChildService.Models
```
and create new folders
```
CompanyName.SampleService.Infrastructure.SampleChildService.Interfaces
CompanyName.SampleService.Infrastructure.SampleChildService.Profiles
CompanyName.SampleService.Infrastructure.SampleChildService.Services
```
##### Add *HttpClient with Polly*

Install *Microsoft.Extensions.Http.Polly* and *System.Net.Http.Json* insite *Infrastructure* project

```dotnetcli
cd ./CompanyName.SampleService.Infrastructure
dotnet add package Microsoft.Extensions.Http.Polly
dotnet add package System.Net.Http.Json
```

Use configuration object for external service

```csharp
namespace CompanyName.SampleService.Infrastructure.SampleChildService
{
    using System;

    internal sealed record SampleChildServiceOptions
    {
        public string SectionName => "SampleChildService";
        public Uri BaseAddress { get; init; } = new Uri("about:blank");
        public int RetryCount { get; init; } = default;
        public int RetrySleepDurationInMilliSeconds { get; set; } = default;
        public int TimeoutInMilliSeconds { get; init; } = default;
    }
}
```

#### Change *CompanyName/SampleService/Infrastructure/DependencyInjection.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure
{
    using System;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Reflection;
    using CompanyName.SampleService.Infrastructure.SampleChildService;
    using CompanyName.SampleService.Infrastructure.SampleChildService.Interfaces;
    using CompanyName.SampleService.Infrastructure.SampleChildService.Services;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Interfaces;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Services;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Polly;
    using Polly.Extensions.Http;

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly()); 
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddSingleton<IWeatherForecastService, WeatherForecastService>();

            var options = new SampleChildServiceOptions();
            configuration.GetSection(options.SectionName).Bind(options);

            services.AddHttpClient("SampleChildService", client =>
            {
                client.BaseAddress = options.BaseAddress;
                client.Timeout = TimeSpan.FromMilliseconds(options.TimeoutInMilliSeconds);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).ConfigureHttpMessageHandlerBuilder(builder =>
            {
#if DEBUG
                builder.PrimaryHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
                };
#endif
            }).AddPolicyHandler(_ =>
            {
                return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(result => result.StatusCode != HttpStatusCode.Accepted)
                .WaitAndRetryAsync(
                    options.RetryCount,
                    retryAttempt => TimeSpan.FromMilliseconds(options.RetrySleepDurationInMilliSeconds)
                );
            });
            services.AddSingleton<ISampleChildService, SampleChildService>();

            return services;
        }
    }
}
```

#### Create *CompanyName/SampleService/Infrastructure/SampleChildService/Models/SampleChildRequest.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure.SampleChildService.Models
{
    internal sealed record SampleChildRequest
    {
    }
}
```
#### Create *CompanyName/SampleService/Infrastructure/SampleChildService/Models/SampleChildResponse.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure.SampleChildService.Models
{
    internal sealed record SampleChildResponse
    {
    }
}
```
#### Create *CompanyName/SampleService/Infrastructure/SampleChildService/Interfaces/ISampleChildService.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure.SampleChildService.Interfaces
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CompanyName.SampleService.Infrastructure.SampleChildService.Models;

    internal interface ISampleChildService
    {
        Task<IReadOnlyList<SampleChildResponse>> GetAsync(SampleChildRequest request, CancellationToken cancellationToken = default);
    }
}
```
#### Create *CompanyName/SampleService/Infrastructure/SampleChildService/Services/SampleChildService.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure.SampleChildService.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using CompanyName.SampleService.Infrastructure.Extensions;
    using CompanyName.SampleService.Infrastructure.SampleChildService.Interfaces;
    using CompanyName.SampleService.Infrastructure.SampleChildService.Models;
    using Microsoft.Extensions.Logging;

    internal sealed class SampleChildService : ISampleChildService
    {
        private const string ClientName = "SampleChildService";

        private readonly HttpClient client;
        private readonly ILogger<SampleChildService> logger;

        public SampleChildService(IHttpClientFactory httpClientFactory, ILogger<SampleChildService> logger) =>
            (this.client, this.logger) = (httpClientFactory.CreateClient(ClientName), logger);

        public async Task<IReadOnlyList<SampleChildResponse>> GetAsync(SampleChildRequest request, CancellationToken cancellationToken = default)
        {
            using var loggerScope = this.logger.BeginPropertyScope(
                ("BaseAddress", $"{this.client.BaseAddress}")
            );

            var requestUri = $"{this.client.BaseAddress}Get";

            try
            {
                var response = await this.client.PostAsJsonAsync(requestUri, request);

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<SampleChildResponse>>();
                return result ?? new List<SampleChildResponse>().AsReadOnly();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, exception.Message);
            }

            return await Task.FromResult(new List<SampleChildResponse>().AsReadOnly());
        }
    }
}
```
#### Create example handler *CompanyName/SampleService/Infrastructure/SampleChildService/QueryHandlers/GetSampleChildServiceRecordsHandler.cs* file
```csharp
namespace CompanyName.SampleService.Infrastructure.SampleChildService.QueryHandlers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CompanyName.SampleService.Application.Queries;
    using CompanyName.SampleService.Infrastructure.SampleChildService.Interfaces;
    using CompanyName.SampleService.Infrastructure.SampleChildService.Models;
    using MediatR;

    internal sealed class GetSampleChildServiceRecordsHandler : IRequestHandler<GetSampleChildServiceRecords, IReadOnlyList<string>>
    {
        private readonly ISampleChildService service;

        public GetSampleChildServiceRecordsHandler(ISampleChildService service) =>
            (this.service) = (service);

        public async Task<IReadOnlyList<string>> Handle(GetSampleChildServiceRecords request, CancellationToken cancellationToken)
        {
            var sampleChildRequest = new SampleChildRequest();
            var sampleChildResponse = await this.service.GetAsync(sampleChildRequest, cancellationToken);

            var response = new List<string>();

            foreach (var sampleChildResponseEntity in sampleChildResponse)
            {
                response.Add(string.Empty);
            }

            return await Task.FromResult(response.AsReadOnly());
        }
    }
}
```
## SSL with Certificates from AWS

#### *Dockerfile* file
```
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY Verus.InternalVerification.sln Verus.InternalVerification.sln
COPY Verus.InternalVerification.Application/Verus.InternalVerification.Application.csproj Verus.InternalVerification.Application/Verus.InternalVerification.Application.csproj
COPY Verus.InternalVerification.Infrastructure/Verus.InternalVerification.Infrastructure.csproj Verus.InternalVerification.Infrastructure/Verus.InternalVerification.Infrastructure.csproj
COPY Verus.InternalVerification.WebApi/Verus.InternalVerification.WebApi.csproj Verus.InternalVerification.WebApi/Verus.InternalVerification.WebApi.csproj
RUN dotnet restore

COPY . .

RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish
RUN cp ca.crt /app/publish/ca.crt

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app

COPY --from=build /app/publish .
RUN mkdir -p /usr/local/share/ca-certificates && cp ca.crt /usr/local/share/ca-certificates/ca.crt
RUN update-ca-certificates

RUN groupadd -g 10000 dotnet && useradd -u 10000 -g dotnet -d /app dotnet && chown -R dotnet:dotnet /app
USER dotnet:dotnet

ENV ASPNETCORE_URLS https://*:5443
EXPOSE 5443

ENTRYPOINT ["dotnet", "Verus.InternalVerification.WebApi.dll"]
```
Line RUN cp ca.crt /app/publish/ca.crt will copy root certificate injected to source code
Line RUN mkdir -p /usr/local/share/ca-certificates && cp ca.crt /usr/local/share/ca-certificates/ca.crt copy root certificate from build stage to runtime
Line RUN update-ca-certificates update list of root certificates inside container

#### *Verus/InternalVerification/WebApi/Program.cs* file
```csharp
namespace Verus.InternalVerification.WebApi
{
    using System;
    using System.Linq;
    using System.Security.Authentication;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    public class Program
    {
        private const int EXIT_FAILURE = 1;
        private const int EXIT_SUCCESS = 0;

        public static async Task<int> Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Application is starting up...");
                await CreateHostBuilder(args).Build().RunAsync();
                return EXIT_SUCCESS;
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Host terminated unexpectedly");
                return EXIT_FAILURE;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
#if !DEBUG
                    webBuilder.UseKestrel(serverOptions =>
                    {
                        serverOptions.ConfigureHttpsDefaults(listenOptions =>
                        {
                            var cert = Base64ToBase64(Environment.GetEnvironmentVariable("SSL_RSA_CERT") ?? string.Empty);
                            var key = Base64ToBase64(Environment.GetEnvironmentVariable("SSL_RSA_KEY") ?? string.Empty);
                            listenOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                            listenOptions.ServerCertificate = BuildCertificate(cert, key);
                        });
                    });
#endif
                    webBuilder.UseStartup<Startup>();
                });

        private static string Base64ToBase64(string source)
        {
            try
            {
                var base64 = source.Replace("\\n", string.Empty).Replace("\n", string.Empty); // Remove new line asci or 'new line' from text
                var bytes = Convert.FromBase64String(base64); // Convert base64 string to bytes array
                var text = Encoding.Default.GetString(bytes); // Convert bytes array to 'original' string
                return text.Replace("\\n", Environment.NewLine).Replace("\"", string.Empty); // Replace 'new line' text to new line, remove quotes
            }
            catch
            {
                return source;
            }
        }

        private static X509Certificate2 BuildCertificate(string cert, string privateKey)
        {
            var pfxWithoutKey = new X509Certificate2(Encoding.Default.GetBytes(cert));

            var lines = privateKey.Split(new[] { "\n", "\r", Environment.NewLine, }, StringSplitOptions.RemoveEmptyEntries);
            var filtered = string.Join(null, lines.Where(x => !x.StartsWith("-")));
            var key = Convert.FromBase64String(filtered);

            var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(key, out _);

            using var pfxWithKey = pfxWithoutKey.CopyWithPrivateKey(rsa);
            var pfx = new X509Certificate2(pfxWithKey.Export(X509ContentType.Pfx), (string)null, X509KeyStorageFlags.PersistKeySet);

            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadWrite);

            try
            {
                store.Add(pfx);
            }
            finally
            {
                store.Close();
            }

            return pfx;
        }
    }
}
```
To use certificate from AWS injected as environment variable to container we need to decode them from Base64 into binary format. In the code I left some comment what is going on. Most importand part is export combined (certificate + private key) and export certificate in this form to local certificate storage. Without this step you can not use combined certificated in Kestrel. We are doing that in BuildCertificate method.

## Unit Tests

If we wont to use tests we should use different structure for the solution. We need to create two folders, one for application (src) and one for tests (tests).
```powershell
mkdir src
mkdir tests
```
Inside src folder we will keep main application and source code tree to build docker image.
Inside tests folder we will keep our tests and we will use this solution for develop.

### Tests

We need to create dedicated solutions for development/tests and add project from main source code tree (from src folder)

```powershell
dotnet new sln --name CompanyName.SampleService.Tests
dotnet sln add ../src/CompanyName.SampleService.Application/CompanyName.SampleService.Application.csproj
dotnet sln add ../src/CompanyName.SampleService.Domain/CompanyName.SampleService.Domain.csproj
dotnet sln add ../src/CompanyName.SampleService.Infrastructure/CompanyName.SampleService.Infrastructure.csproj
dotnet sln add ../src/CompanyName.SampleService.WebApi/CompanyName.SampleService.WebApi.csproj
```

We can create our unit test project
```powershell
dotnet new mstest --output CompanyName.SampleService.UnitTests
dotnet sln add ./CompanyName.SampleService.UnitTests/CompanyName.SampleService.UnitTests.csproj
cd CompanyName.SampleService.UnitTests
dotnet add reference ../../src/CompanyName.SampleService.Application/CompanyName.SampleService.Application.csproj
dotnet add reference ../../src/CompanyName.SampleService.Domain/CompanyName.SampleService.Domain.csproj
dotnet add reference ../../src/CompanyName.SampleService.Infrastructure/CompanyName.SampleService.Infrastructure.csproj
```
We need to add some nuget packages
```powershell
dotnet add package FluentAssertions
dotnet add package Moq
```

Because we are using limited visibility for handlers we need to modify our *.csproj files

### Change */src/CompanyName.SampleService.Application/CompanyName.SampleService.Application.csproj* file
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <InternalsVisibleTo Include="CompanyName.SampleService.UnitTests" />
    <InternalsVisibleTo Include="DynamicProxyGenAssembly2" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\CompanyName.SampleService.Domain\CompanyName.SampleService.Domain.csproj" />
  </ItemGroup>

</Project>
```
### Change */src/CompanyName.SampleService.Infrastructure/CompanyName.SampleService.Infrastructure.csproj* file
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <InternalsVisibleTo Include="CompanyName.SampleService.UnitTests" />
    <InternalsVisibleTo Include="DynamicProxyGenAssembly2" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="8.1.1" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="5.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="5.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\CompanyName.SampleService.Application\CompanyName.SampleService.Application.csproj" />
  </ItemGroup>

</Project>
```
Now we need to create first unit test
### Create */tests/CompanyName.SampleService.UnitTests/WeatherForecastsTests.cs* file
```csharp
namespace CompanyName.SampleService.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using CompanyName.SampleService.Application.Queries;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Interfaces;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Models;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Profiles;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.QueryHandlers;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public sealed class WeatherForecastsTests
    {
        public TestContext TestContext { get; set; }

        private static readonly string[] Summaries = new[]
        {
            "Balmy",
            "Bracing",
            "Chilly",
            "Cool",
            "Freezing",
            "Hot",
            "Mild",
            "Warm",
            "Scorching",
            "Sweltering",
        };

        [TestMethod]
        public async Task Get_List_of_WeatherForecasts()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<WeatherForecastProfile>());
            var mapper = config.CreateMapper();

            var weatherForecasts = new List<WeatherForecast>
            {
                new WeatherForecast{
                    Date = new DateTime(2021, 01, 01),
                    Summary = "Cool",
                    TemperatureC = -20,
                },
                new WeatherForecast{
                    Date = new DateTime(2021, 07, 22),
                    Summary = "Warm",
                    TemperatureC = 28,
                },
            };

            var service = new Mock<IWeatherForecastService>();
            service.Setup(m => m.Get(CancellationToken.None)).ReturnsAsync(weatherForecasts);

            //Arrange
            var query = new GetWeatherForecasts();
            var handler = new GetWeatherForecastsHandler(mapper, service.Object);

            //Act
            var result = await handler.Handle(query, CancellationToken.None);
            this.TestContext.WriteLine(result.ToString());

            //Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);

            foreach (var item in result)
            {
                item.Summary.Should().BeOneOf(Summaries);
                item.TemperatureC.Should().BeInRange(-20, 55);
            }
        }

        [TestMethod]
        public async Task TestException()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<WeatherForecastProfile>());
            var mapper = config.CreateMapper();

            var weatherForecasts = new List<WeatherForecast>
            {
                new WeatherForecast{
                    Date = new DateTime(2021, 01, 01),
                    Summary = "Cool",
                    TemperatureC = -20,
                },
                new WeatherForecast{
                    Date = new DateTime(2021, 07, 22),
                    Summary = "Warm",
                    TemperatureC = 28,
                },
            };

            var service = new Mock<IWeatherForecastService>();
            service.Setup(m => m.Get(CancellationToken.None)).ReturnsAsync(weatherForecasts);

            //Arrange
            GetWeatherForecasts query = null;
            var handler = new GetWeatherForecastsHandler(mapper, service.Object);

            //Act
            Func<Task<IReadOnlyList<CompanyName.SampleService.Application.ViewModels.WeatherForecast>>> action = async () => await handler.Handle(query, CancellationToken.None);

            //Assert
            await action.Should().ThrowExactlyAsync<NullReferenceException>();
        }
    }
}
```
Now we can check our test
```powershell
dotnet test
```
## Integration Tests
We can create our unit test project
```powershell
dotnet new mstest --output CompanyName.SampleService.IntegrationTests
dotnet sln add ./CompanyName.SampleService.IntegrationTests/CompanyName.SampleService.IntegrationTests.csproj
cd CompanyName.SampleService.IntegrationTests
dotnet add reference ../../src/CompanyName.SampleService.WebApi/CompanyName.SampleService.WebApi.csproj
```
We need to add some nuget packages
```powershell
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.TestHost
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Moq
```
```csharp
namespace CompanyName.SampleService.IntegrationTests
{
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using CompanyName.SampleService.WebApi;
    using Serilog;

    [TestClass]
    public class IntegrationTests
    {
        private readonly HttpClient client;
        private readonly TestServer server;
        public TestContext TestContext { get; set; }

        public IntegrationTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var build = new WebHostBuilder()
                .UseContentRoot(Path.GetDirectoryName(Assembly.GetAssembly(typeof(Startup)).Location))
                .UseConfiguration(configuration)
                .ConfigureTestServices(services =>
                {
                })
                .UseSerilog()
                .UseStartup<Startup>();
            this.server = new TestServer(build);
            this.client = this.server.CreateClient();
        }

        [TestMethod]
        public async Task WeatherForecastControllerTest()
        {
            //Arrange

            //Act
            var json = await this.client.GetStringAsync("/WeatherForecast");

            //Assert
            TestContext.WriteLine(json);
            json.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public async Task WeatherForecastControllerEndpointNotFoundTest()
        {
            //Arrange

            //Act
            var response = await this.client.GetAsync("/NotExists");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task HealthChecksTest()
        {
            //Arrange

            //Act
            var response = await this.client.GetStringAsync("/health");

            //Assert
            response.Should().NotBeNullOrWhiteSpace();
            response.Should().Equals("Healthy");
        }
    }
}
```
Now we can check our test
```powershell
dotnet test -l "console;verbosity=detailed"
```
Create *extensions* folder
# SonarQube
## SonarQube Server
```powershell
mkdir extensions
```
Create volumens
```powershell
docker volume create sonar-data
docker volume create sonar-logs
```
### Create *docker-compose.yml* file
```
version: "3.9"
volumes:
    sonar-data:
        external: true
    sonar-logs:
        external: true
services:
    sonarqube:
        image: sonarqube:9.0.1-community
        ports:
            - "9000:9000"
        volumes:
            - sonar-data:/opt/sonarqube/data
            - sonar-logs:/opt/sonarqube/logs
            - ./extensions:/opt/sonarqube/extensions
```
Now we can stert the service.
```powershell
docker compose up -d
```
Use favorites web browser and navigate to http://localhost:9000. After login/set up password please create new project (ex: SampleService).

## Sonar Scanner
```powershell
dotnet tool install --global dotnet-sonarscanner
```
Now we can run scanner.
```powershell
dotnet sonarscanner begin /k:"SampleService" /d:sonar.login="user" /d:sonar.password="password" /d:sonar.host.url="http://localhost:9000"
dotnet build CompanyName.SampleService.sln
dotnet sonarscanner end /d:sonar.login="user" /d:sonar.password="password"
```
Use favorites web browser and navigate to http://localhost:9000. You can see some guidelines how to make your code more secure. Following the instruction given by SonarQube we can improve our code.

### Change *./src/CompanyName.SampleService.WebApi/SerilogLoggingActionFilter.cs* file to
```csharp
namespace CompanyName.SampleService.WebApi
{
    using System;
    using System.Diagnostics.Contracts;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Serilog;

    public sealed class SerilogLoggingActionFilter : IActionFilter
    {
        private readonly IDiagnosticContext diagnosticContext;
        public SerilogLoggingActionFilter(IDiagnosticContext diagnosticContext)
        {
            this.diagnosticContext = diagnosticContext
                ?? throw new ArgumentNullException(nameof(diagnosticContext));
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            Contract.Assert(context != null);
            this.diagnosticContext.Set("ActionId", context.ActionDescriptor.Id);
            this.diagnosticContext.Set("ActionName", context.ActionDescriptor.DisplayName);
            this.diagnosticContext.Set("RouteData", context.ActionDescriptor.RouteValues);
            this.diagnosticContext.Set("ValidationState", context.ModelState.IsValid);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Method intentionally left empty.
        }
    }
}
```
### Change *./src/CompanyName.SampleService.Infrastructure.WeatherForecasts.Services/WeatherForecastService.cs* file to
```csharp
namespace CompanyName.SampleService.Infrastructure.WeatherForecasts.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Interfaces;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Models;

    internal sealed class WeatherForecastService : IWeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing",
            "Bracing",
            "Chilly",
            "Cool",
            "Mild",
            "Warm",
            "Balmy",
            "Hot",
            "Sweltering",
            "Scorching",
        };

        public async Task<IReadOnlyList<WeatherForecast>> Get(CancellationToken cancellationToken = default)
        {
            var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = RandomNumberGenerator.GetInt32(-20, 55),
                Summary = Summaries[RandomNumberGenerator.GetInt32(Summaries.Length)]
            })
            .ToList();

            return await Task.FromResult(result.AsReadOnly());
        }
    }
}
```
