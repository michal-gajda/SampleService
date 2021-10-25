namespace CompanyName.SampleService.Infrastructure.WeatherForecasts.Interfaces
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Models;

    internal interface IWeatherForecastService
    {
        Task<IReadOnlyList<WeatherForecast>> GetAsync(int count = default, CancellationToken cancellationToken = default);
    }
}
