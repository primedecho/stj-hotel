using FluentValidation;
using HotelSearch.Api.Contracts.Hotels;

namespace HotelSearch.Api.Validation;

public sealed class UpdateHotelRequestValidator : AbstractValidator<UpdateHotelRequest>
{
    public UpdateHotelRequestValidator()
    {
        HotelRequestValidationRules.Apply(
            this,
            x => x.Name,
            x => x.Price,
            x => x.Latitude,
            x => x.Longitude);
    }
}
