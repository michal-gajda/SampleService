namespace CompanyName.SampleService.WebApi.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Mvc;

    [ExcludeFromCodeCoverage]
    public sealed class DefaultController : Controller
    {
        [Route(""), HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public RedirectResult RedirectToSwaggerUi()
        {
            return Redirect("swagger/index.html");
        }
    }
}
