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
