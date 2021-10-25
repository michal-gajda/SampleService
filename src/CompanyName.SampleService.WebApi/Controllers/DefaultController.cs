namespace CompanyName.SampleService.WebApi.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Mvc;

    [ExcludeFromCodeCoverage]
    public sealed class DefaultController : Controller
    {
        [ApiExplorerSettings(IgnoreApi = true), HttpGet, Route("")]
        public RedirectResult RedirectToSwaggerUi()
        {
            return Redirect("swagger/index.html");
        }
    }
}
