namespace CompanyName.SampleService.WebApi.Extensions
{
    using System.Diagnostics.CodeAnalysis;
    using CompanyName.SampleService.WebApi.Middlewares;
    using Microsoft.AspNetCore.Builder;

    [ExcludeFromCodeCoverage]
    internal static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
