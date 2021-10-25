namespace CompanyName.SampleService.WebApi.Controllers
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Mime;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [AllowAnonymous, ApiController, ApiExplorerSettings(IgnoreApi = true), Produces(MediaTypeNames.Application.Json), ExcludeFromCodeCoverage, Route("[controller]")]
    public sealed class VersionController : ControllerBase
    {
        [HttpGet, Route("")]
        public async Task<FileVersionInfo> GetAsync(CancellationToken cancellationToken = default)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var result = FileVersionInfo.GetVersionInfo(assembly.Location);
            return await Task.FromResult(result);
        }
    }
}
