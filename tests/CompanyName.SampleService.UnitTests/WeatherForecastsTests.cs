namespace CompanyName.SampleService.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using CompanyName.SampleService.Application.Queries;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Interfaces;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Models;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.Profiles;
    using CompanyName.SampleService.Infrastructure.WeatherForecasts.QueryHandlers;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public sealed class WeatherForecastsTests
    {
        public TestContext TestContext { get; set; }

        private static readonly string[] Summaries = new[]
        {
            "Balmy",
            "Bracing",
            "Chilly",
            "Cool",
            "Freezing",
            "Hot",
            "Mild",
            "Warm",
            "Scorching",
            "Sweltering",
        };

        [TestMethod]
        public async Task Get_List_of_WeatherForecasts()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<WeatherForecastProfile>());
            var mapper = config.CreateMapper();

            var weatherForecasts = new List<WeatherForecast>
            {
                new WeatherForecast{
                    Date = new DateTime(2021, 01, 01),
                    Summary = "Cool",
                    TemperatureC = -20,
                },
                new WeatherForecast{
                    Date = new DateTime(2021, 07, 22),
                    Summary = "Warm",
                    TemperatureC = 28,
                },
            };

            var service = new Mock<IWeatherForecastService>();
            service.Setup(m => m.Get(CancellationToken.None)).ReturnsAsync(weatherForecasts);

            //Arrange
            var query = new GetWeatherForecasts();
            var handler = new GetWeatherForecastsHandler(mapper, service.Object);

            //Act
            var result = await handler.Handle(query, CancellationToken.None);
            this.TestContext.WriteLine(result.ToString());

            //Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);

            foreach (var item in result)
            {
                item.Summary.Should().BeOneOf(Summaries);
                item.TemperatureC.Should().BeInRange(-20, 55);
            }
        }

        [TestMethod]
        public async Task TestException()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<WeatherForecastProfile>());
            var mapper = config.CreateMapper();

            var weatherForecasts = new List<WeatherForecast>
            {
                new WeatherForecast{
                    Date = new DateTime(2021, 01, 01),
                    Summary = "Cool",
                    TemperatureC = -20,
                },
                new WeatherForecast{
                    Date = new DateTime(2021, 07, 22),
                    Summary = "Warm",
                    TemperatureC = 28,
                },
            };

            var service = new Mock<IWeatherForecastService>();
            service.Setup(m => m.Get(CancellationToken.None)).ReturnsAsync(weatherForecasts);

            //Arrange
            GetWeatherForecasts query = null;
            var handler = new GetWeatherForecastsHandler(mapper, service.Object);

            //Act
            Func<Task<IReadOnlyList<CompanyName.SampleService.Application.ViewModels.WeatherForecast>>> action = async () => await handler.Handle(query, CancellationToken.None);

            //Assert
            await action.Should().ThrowExactlyAsync<NullReferenceException>();
        }
    }
}
