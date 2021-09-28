namespace CompanyName.SampleService.WebApi.Controllers
{
    using System.Collections.Generic;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using CompanyName.SampleService.Application.Queries;
    using CompanyName.SampleService.Application.ViewModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController, Produces(MediaTypeNames.Application.Json), Route("[controller]")]
    public sealed class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> logger;
        private readonly IMediator mediator;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMediator mediator) =>
            (this.logger, this.mediator) = (logger, mediator);

        [HttpGet(Name = "Get"), ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WeatherForecast>))]
        public async Task<IEnumerable<WeatherForecast>> GetAsync([FromQuery(Name = "count")] int count, CancellationToken cancellationToken = default) =>
            await this.mediator.Send(new GetWeatherForecasts { Count = count, }, cancellationToken);

        [HttpPost(Name = "Post"), Consumes(MediaTypeNames.Application.Json), ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WeatherForecast>))]
        public async Task<IEnumerable<WeatherForecast>> PostAsync([FromBody] GetWeatherForecasts query, CancellationToken cancellationToken = default) =>
            await this.mediator.Send(query, cancellationToken);
    }
}