using FluentValidation;
using FluentValidation.Results;

namespace HotelSearch.Api.Infrastructure;

internal sealed class FluentValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        foreach (var argument in context.Arguments)
        {
            if (argument is null)
            {
                continue;
            }

            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            ValidationResult result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (result.IsValid)
            {
                continue;
            }

            var errors = ApiProblemDetails.ToErrorDictionary(
                result.Errors.Select(e => (e.PropertyName, e.ErrorMessage)));

            var problem = ApiProblemDetails.CreateValidationProblem(context.HttpContext, errors);
            return Results.Json(problem, statusCode: StatusCodes.Status400BadRequest);
        }

        return await next(context);
    }
}
