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