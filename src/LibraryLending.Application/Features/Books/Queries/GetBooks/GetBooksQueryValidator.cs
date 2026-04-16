using FluentValidation;

namespace LibraryLending.Application.Features.Books.Queries.GetBooks;

public sealed class GetBooksQueryValidator : AbstractValidator<GetBooksQuery>
{
    private static readonly int MaxPublicationYear = DateTime.UtcNow.Year + 1;

    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "title",
        "author",
        "publicationyear",
        "isbn",
        "availablecopies",
        "createdat",
        "updatedat"
    };

    public GetBooksQueryValidator()
    {
        RuleFor(x => x.Filter)
            .NotNull();

        When(x => x.Filter is not null, () =>
        {
            RuleFor(x => x.Filter.SearchTerm)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Filter.SearchTerm));

            RuleFor(x => x.Filter.Title)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Filter.Title));

            RuleFor(x => x.Filter.Author)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Filter.Author));

            RuleFor(x => x.Filter.Isbn)
                .MaximumLength(32)
                .When(x => !string.IsNullOrWhiteSpace(x.Filter.Isbn));

            RuleFor(x => x.Filter.PublicationYearFrom)
                .InclusiveBetween(1450, MaxPublicationYear)
                .When(x => x.Filter.PublicationYearFrom.HasValue);

            RuleFor(x => x.Filter.PublicationYearTo)
                .InclusiveBetween(1450, MaxPublicationYear)
                .When(x => x.Filter.PublicationYearTo.HasValue);

            RuleFor(x => x.Filter)
                .Must(filter => !filter.PublicationYearFrom.HasValue || !filter.PublicationYearTo.HasValue || filter.PublicationYearFrom <= filter.PublicationYearTo)
                .WithMessage("PublicationYearFrom cannot be greater than PublicationYearTo.");

            RuleFor(x => x.Filter.SortBy)
                .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || AllowedSortFields.Contains(sortBy.Trim()))
                .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");
        });
    }
}
