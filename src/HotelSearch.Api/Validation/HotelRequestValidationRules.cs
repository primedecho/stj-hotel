using System.Linq.Expressions;
using FluentValidation;

namespace HotelSearch.Api.Validation;

internal static class HotelRequestValidationRules
{
    public static void Apply<T>(
        AbstractValidator<T> validator,
        Expression<Func<T, string>> nameSelector,
        Expression<Func<T, decimal>> priceSelector,
        Expression<Func<T, double>> latitudeSelector,
        Expression<Func<T, double>> longitudeSelector)
    {
        validator.RuleFor(nameSelector)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Hotel name cannot be empty.")
            .MaximumLength(200)
            .WithMessage("Hotel name cannot exceed 200 characters.");

        validator.RuleFor(priceSelector)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero.");

        validator.RuleFor(latitudeSelector)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90.");

        validator.RuleFor(longitudeSelector)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180.");
    }
}
