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

        public async Task<IReadOnlyList<WeatherForecast>> GetAsync(int count = default, CancellationToken cancellationToken = default)
        {
            var result = Enumerable.Range(1, count).Select(index => new WeatherForecast
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
