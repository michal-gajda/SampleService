namespace CompanyName.SampleService.IntegrationTests
{
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using CompanyName.SampleService.WebApi;
    using FluentAssertions;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Serilog;

    [TestClass]
    public class IntegrationTests
    {
        private readonly HttpClient client;
        private readonly TestServer server;
        public TestContext TestContext { get; set; }

        public IntegrationTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var build = new WebHostBuilder()
                .UseContentRoot(Path.GetDirectoryName(Assembly.GetAssembly(typeof(Startup)).Location))
                .UseConfiguration(configuration)
                .ConfigureTestServices(services =>
                {
                })
                .UseSerilog()
                .UseStartup<Startup>();
            this.server = new TestServer(build);
            this.client = this.server.CreateClient();
        }

        [TestMethod]
        public async Task WeatherForecastControllerTest()
        {
            //Arrange

            //Act
            var json = await this.client.GetStringAsync("/WeatherForecast");

            //Assert
            TestContext.WriteLine(json);
            json.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public async Task WeatherForecastControllerEndpointNotFoundTest()
        {
            //Arrange

            //Act
            var response = await this.client.GetAsync("/NotExists");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task HealthChecksTest()
        {
            //Arrange

            //Act
            var response = await this.client.GetStringAsync("/health");

            //Assert
            response.Should().NotBeNullOrWhiteSpace();
            response.Should().BeEquivalentTo("Healthy");
        }
    }
}
