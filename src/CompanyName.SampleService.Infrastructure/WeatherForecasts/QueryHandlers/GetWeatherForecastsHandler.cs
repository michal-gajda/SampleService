namespace CompanyName.SampleService.Infrastructure.WeatherForecasts.QueryHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using CompanyName.SampleService.Application.Queries;
    using CompanyName.SampleService.Application.ViewModels;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Interfaces;
    using MediatR;
    using Microsoft.Extensions.Logging;

    internal sealed class GetWeatherForecastsHandler : IRequestHandler<GetWeatherForecasts, IReadOnlyList<WeatherForecast>>
    {
        private readonly ILogger<GetWeatherForecastsHandler> logger;
        private readonly IMapper mapper;
        private readonly IWeatherForecastService service;

        public GetWeatherForecastsHandler(ILogger<GetWeatherForecastsHandler> logger, IMapper mapper, IWeatherForecastService service) =>
            (this.logger, this.mapper, this.service) = (logger, mapper, service);

        public async Task<IReadOnlyList<WeatherForecast>> Handle(GetWeatherForecasts request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var source = await this.service.GetAsync(request.Count, cancellationToken);
            var result = this.mapper.Map<IReadOnlyList<WeatherForecast>>(source);
            return result;
        }
    }
}
