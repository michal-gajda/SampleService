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