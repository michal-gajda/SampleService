namespace CompanyName.SampleService.Application.ViewModels
{
    using System;
    using System.Text.Json.Serialization;

    public sealed record WeatherForecast
    {
        [JsonPropertyName("date")] public DateTime Date { get; init; }
        [JsonPropertyName("temperatureInC")] public int TemperatureC { get; init; }
        [JsonPropertyName("temperatureInF")] public int TemperatureF { get; init; }
        [JsonPropertyName("summary")] public string Summary { get; init; } = string.Empty;
    }
}
