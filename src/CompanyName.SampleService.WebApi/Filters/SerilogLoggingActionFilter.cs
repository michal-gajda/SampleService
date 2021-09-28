namespace CompanyName.SampleService.WebApi.Filters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Serilog;

    [ExcludeFromCodeCoverage]
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class SerilogLoggingActionFilter : IActionFilter
    {
        private readonly IDiagnosticContext diagnosticContext;

        public SerilogLoggingActionFilter(IDiagnosticContext diagnosticContext)
        {
            this.diagnosticContext = diagnosticContext
                ?? throw new ArgumentNullException(nameof(diagnosticContext));
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            Contract.Assert(context is not null);
            this.diagnosticContext.Set("ActionId", context.ActionDescriptor.Id);
            this.diagnosticContext.Set("ActionName", context.ActionDescriptor.DisplayName);
            this.diagnosticContext.Set("RouteData", context.ActionDescriptor.RouteValues);
            this.diagnosticContext.Set("ValidationState", context.ModelState.IsValid);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Method intentionally left empty.
        }
    }
}
