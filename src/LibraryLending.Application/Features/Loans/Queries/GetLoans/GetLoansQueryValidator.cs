using FluentValidation;

namespace LibraryLending.Application.Features.Loans.Queries.GetLoans;

public sealed class GetLoansQueryValidator : AbstractValidator<GetLoansQuery>
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "loandate",
        "duedate",
        "returnedat",
        "status",
        "createdat",
        "updatedat"
    };

    public GetLoansQueryValidator()
    {
        RuleFor(x => x.Filter)
            .NotNull();

        When(x => x.Filter is not null, () =>
        {
            RuleFor(x => x.Filter.SearchTerm)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Filter.SearchTerm));

            RuleFor(x => x.Filter.BookId)
                .Must(id => !id.HasValue || id.Value != Guid.Empty)
                .WithMessage("BookId must be a non-empty GUID when provided.");

            RuleFor(x => x.Filter.MemberId)
                .Must(id => !id.HasValue || id.Value != Guid.Empty)
                .WithMessage("MemberId must be a non-empty GUID when provided.");

            RuleFor(x => x.Filter)
                .Must(filter => !filter.LoanDateFromUtc.HasValue || !filter.LoanDateToUtc.HasValue || filter.LoanDateFromUtc <= filter.LoanDateToUtc)
                .WithMessage("LoanDateFromUtc cannot be greater than LoanDateToUtc.");

            RuleFor(x => x.Filter)
                .Must(filter => !filter.DueDateFromUtc.HasValue || !filter.DueDateToUtc.HasValue || filter.DueDateFromUtc <= filter.DueDateToUtc)
                .WithMessage("DueDateFromUtc cannot be greater than DueDateToUtc.");

            RuleFor(x => x.Filter.SortBy)
                .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || AllowedSortFields.Contains(sortBy.Trim()))
                .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");
        });
    }
}
