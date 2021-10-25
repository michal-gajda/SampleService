namespace CompanyName.SampleService.IntegrationTests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CompanyName.SampleService.Application.ViewModels;
    using CompanyName.SampleService.WebApi;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var response = await client.GetAsync("/WeatherForecast?count=5");
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
