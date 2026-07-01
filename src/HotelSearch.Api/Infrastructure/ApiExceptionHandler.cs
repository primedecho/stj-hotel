using HotelSearch.Application.Common;
using HotelSearch.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace HotelSearch.Api.Infrastructure;

internal sealed class ApiExceptionHandler(
    IHostEnvironment environment,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, type) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found", ApiProblemDetails.NotFoundType),
            DomainException or AppException => (StatusCodes.Status400BadRequest, "Bad Request", ApiProblemDetails.ValidationType),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", ApiProblemDetails.ServerErrorType)
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        var detail = statusCode == StatusCodes.Status500InternalServerError && !environment.IsDevelopment()
            ? "An unexpected error occurred."
            : exception.Message;

        var problem = ApiProblemDetails.CreateProblem(
            httpContext,
            statusCode,
            title,
            detail,
            type);

        if (environment.IsDevelopment() && statusCode == StatusCodes.Status500InternalServerError)
        {
            problem.Extensions["exceptionType"] = exception.GetType().Name;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
