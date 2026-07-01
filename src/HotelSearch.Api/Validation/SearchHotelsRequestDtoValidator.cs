using FluentValidation;
using HotelSearch.Api.Contracts.Search;
using HotelSearch.Application.Search;

namespace HotelSearch.Api.Validation;

public sealed class SearchHotelsRequestDtoValidator : AbstractValidator<SearchHotelsRequestDto>
{
    public SearchHotelsRequestDtoValidator()
    {
        RuleFor(x => x.Prompt)
            .Must(prompt => !string.IsNullOrWhiteSpace(prompt))
            .WithMessage("Search prompt cannot be empty.")
            .MaximumLength(SearchConstants.PromptMaxLength)
            .WithMessage($"Search prompt cannot exceed {SearchConstants.PromptMaxLength} characters.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(SearchConstants.MaxPageSize)
            .WithMessage($"Page size cannot exceed {SearchConstants.MaxPageSize}.");
    }
}
