using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HotelSearch.Api.Infrastructure;

internal static class ApiProblemDetails
{
    public const string ValidationType = "https://tools.ietf.org/html/rfc9110#section-15.5.1";
    public const string NotFoundType = "https://tools.ietf.org/html/rfc9110#section-15.5.5";
    public const string UnauthorizedType = "https://tools.ietf.org/html/rfc9110#section-15.5.2";
    public const string ServerErrorType = "https://tools.ietf.org/html/rfc9110#section-15.6.1";

    public static ValidationProblemDetails CreateValidationProblem(
        HttpContext httpContext,
        IDictionary<string, string[]> errors,
        string detail = "One or more validation errors occurred.")
    {
        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Detail = detail,
            Type = ValidationType,
            Instance = httpContext.Request.Path
        };

        Enrich(problem, httpContext);
        return problem;
    }

    public static ProblemDetails CreateProblem(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail,
        string type)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = httpContext.Request.Path
        };

        Enrich(problem, httpContext);
        return problem;
    }

    public static Dictionary<string, string[]> ToErrorDictionary(
        IEnumerable<(string PropertyName, string Message)> failures) =>
        failures
            .GroupBy(f => f.PropertyName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Message).Distinct().ToArray(),
                StringComparer.OrdinalIgnoreCase);

    private static void Enrich(ProblemDetails problem, HttpContext httpContext)
    {
        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
    }
}
