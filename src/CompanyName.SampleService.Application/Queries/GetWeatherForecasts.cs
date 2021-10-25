namespace CompanyName.SampleService.Application.Queries
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using CompanyName.SampleService.Application.ViewModels;
    using MediatR;

    public sealed record GetWeatherForecasts : IRequest<IReadOnlyList<WeatherForecast>>
    {
        [JsonPropertyName("count")] public int Count { get; init; } = default;
    }
}
