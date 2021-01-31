using Core.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Linq;

namespace Core.Api.Serilog
{
    public static class LogEnricher
    {
        /// <summary>
        /// Enriches the HTTP request log with additional data via the Diagnostic Context
        /// </summary>
        /// <param name="diagnosticContext">The Serilog diagnostic context</param>
        /// <param name="httpContext">The current HTTP Context</param>
        /// NOTE(collin): https://benfoster.io/blog/serilog-best-practices/#request-log-enricher
        public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress.ToString());
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());

            // NOTE(collin): enable if you want to specify an alternate name in the [Http*] attributes for endpoints
            //diagnosticContext.Set("Resource", httpContext.GetMetricsCurrentResourceName());
        }
    }
}
