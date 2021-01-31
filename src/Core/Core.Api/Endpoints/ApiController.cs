using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;

namespace Core.Api.Endpoints
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        [NonAction]
        public ObjectResult NotFoundProblem(string detail)
        {
            if (string.IsNullOrWhiteSpace(detail))
                throw new ArgumentException($"'{nameof(detail)}' cannot be null or whitespace", nameof(detail));

            var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                detail: detail,
                instance: UriHelper.GetDisplayUrl(Request));

            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status
            };
        }
    }
}
