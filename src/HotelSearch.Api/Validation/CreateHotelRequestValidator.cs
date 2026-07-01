using FluentValidation;
using HotelSearch.Api.Contracts.Hotels;

namespace HotelSearch.Api.Validation;

public sealed class CreateHotelRequestValidator : AbstractValidator<CreateHotelRequest>
{
    public CreateHotelRequestValidator()
    {
        HotelRequestValidationRules.Apply(
            this,
            x => x.Name,
            x => x.Price,
            x => x.Latitude,
            x => x.Longitude);
    }
}
