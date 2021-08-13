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
