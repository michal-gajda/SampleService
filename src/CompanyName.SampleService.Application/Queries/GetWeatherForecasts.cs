namespace CompanyName.SampleService.Application.Queries
{
    using System.Collections.Generic;
    using CompanyName.SampleService.Application.ViewModels;
    using MediatR;

    public sealed record GetWeatherForecasts : IRequest<IReadOnlyList<WeatherForecast>>
    {
    }
}
