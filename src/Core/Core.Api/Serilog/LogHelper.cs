using Core.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Serilog.Events;
using System;

namespace Core.Api.Serilog
{
    public static class LogHelper
    {
        // NOTE(collin): https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-excluding-health-check-endpoints-from-serilog-request-logging/
        public static LogEventLevel ExcludeHealthChecks(HttpContext httpContext, double _, Exception ex)
        {
            if (httpContext is null)
                return LogEventLevel.Error;

            if (ex is object)
                return LogEventLevel.Error;

            if (httpContext.Response.StatusCode > 499)
                return LogEventLevel.Error;

            if (httpContext.IsHealthCheckEndpoint())
                return LogEventLevel.Verbose;

            return LogEventLevel.Information;
        }
    }
}
