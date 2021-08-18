namespace CompanyName.SampleService.IntegrationTests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Text.Json;
    using FluentAssertions;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using CompanyName.SampleService.WebApi;
    using CompanyName.SampleService.Application.ViewModels;

    [TestClass]
    public sealed class IntegrationTestsByFactory
    {
        private readonly WebApplicationFactory<Startup> factory;
        public TestContext TestContext { get; set; }

        public IntegrationTestsByFactory()
        {
            this.factory = new WebApplicationFactory<Startup>();
        }

        [TestMethod]
        public async Task WeatherForecastControllerTest()
        {
            //Arrange
            var client = factory.CreateClient();

            //Act
            var response = await client.GetAsync("/WeatherForecast");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            //Assert
            TestContext.WriteLine(json);
            json.Should().NotBeNullOrWhiteSpace();
            var @object = JsonSerializer.Deserialize<IReadOnlyList<WeatherForecast>>(json);
            @object.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task WeatherForecastControllerEndpointNotFoundTest()
        {
            //Arrange
            var client = factory.CreateClient();

            //Act
            var response = await client.GetAsync("/NotExists");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task HealthChecksTest()
        {
            //Arrange
            var client = factory.CreateClient();

            //Act
            var response = await client.GetStringAsync("/health");

            //Assert
            response.Should().NotBeNullOrWhiteSpace();
            response.Should().BeEquivalentTo("Healthy");
        }
    }
}
