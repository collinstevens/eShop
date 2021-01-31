using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System;

namespace Core.Api.Serilog
{
    // NOTE(collin): https://nblumhardt.com/2019/10/serilog-mvc-logging/
    // NOTE(collin): https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-logging-mvc-propertis-with-serilog/#logging-mvc-properties-with-a-custom-action-filter
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SerilogLoggingAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var diagnosticContext = (IDiagnosticContext)context.HttpContext.RequestServices.GetService(typeof(IDiagnosticContext));
            diagnosticContext.Set("ActionName", context.ActionDescriptor.DisplayName);
            diagnosticContext.Set("ActionId", context.ActionDescriptor.Id);
            diagnosticContext.Set("RouteData", context.ActionDescriptor.RouteValues);
            diagnosticContext.Set("ValidationState", context.ModelState.IsValid);
        }
    }
}
