using BookingService.Domain.CustomExceptions;
using EventService.Domain.CustomExceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserService.Domain.CustomExceptions;

namespace EventApp.Shared.Exceptions
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleException(httpContext, ex);
            }
        }

        private async Task HandleException(HttpContext httpContext, Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception. Method={Method}, Path={Path}, RequestId={RequestId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Request.Headers["x-request-id"]);

            if (httpContext.Response.HasStarted)
            {
                return;
            }

            var statusCode = MapStatusCode(ex);

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var error = new ProblemDetails
            {
                Status = statusCode,
                Detail = ex.Message
            };

            await httpContext.Response.WriteAsJsonAsync(error);
        }

        private static int MapStatusCode(Exception ex)
            => ex switch
            {
                ValidationUserException ve => StatusCodes.Status400BadRequest,
                ValidationEventException ve => StatusCodes.Status400BadRequest,
                ValidationBookingException ve => StatusCodes.Status400BadRequest,
                NotFoundUserException nfue => StatusCodes.Status404NotFound,
                NotFoundEventException nfee => StatusCodes.Status404NotFound,
                NotFoundBookingException nfee => StatusCodes.Status404NotFound,
                NoAvailableSeatsException nase => StatusCodes.Status409Conflict,
                ActiveLeasesExceededException alee => StatusCodes.Status409Conflict,
                PastEventBookingException pebe => StatusCodes.Status400BadRequest,
                PermissionDeniedException pde => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };
    }
}
