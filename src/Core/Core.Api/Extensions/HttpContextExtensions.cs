using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace Core.Api.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the current API resource name from HTTP context
        /// </summary>
        /// <param name="httpContext">The HTTP context</param>
        /// <returns>The current resource name if available, otherwise an empty string</returns>
        /// NOTE(collin): https://benfoster.io/blog/aspnetcore-3-1-current-route-endpoint-name/
        /// not used currently because the ActionId and ActionName cover the use case from SerilogLoggingAttribute.cs
        public static string GetMetricsCurrentResourceName(this HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            Endpoint endpoint = httpContext.GetEndpoint();
            return endpoint?.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName;
        }

        // NOTE(collin): https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-excluding-health-check-endpoints-from-serilog-request-logging/
        public static bool IsHealthCheckEndpoint(this HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint();
            if (endpoint is null)
                return false;

            return string.Equals(endpoint.DisplayName, "Health checks", StringComparison.OrdinalIgnoreCase);
        }
    }
}
