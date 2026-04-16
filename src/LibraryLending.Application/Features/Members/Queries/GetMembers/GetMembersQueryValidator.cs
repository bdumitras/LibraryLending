using FluentValidation;

namespace LibraryLending.Application.Features.Members.Queries.GetMembers;

public sealed class GetMembersQueryValidator : AbstractValidator<GetMembersQuery>
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "fullname",
        "email",
        "createdat",
        "updatedat"
    };

    public GetMembersQueryValidator()
    {
        RuleFor(x => x.Filter)
            .NotNull();

        When(x => x.Filter is not null, () =>
        {
            RuleFor(x => x.Filter.SearchTerm)
                .MaximumLength(150)
                .When(x => !string.IsNullOrWhiteSpace(x.Filter.SearchTerm));

            RuleFor(x => x.Filter.FullName)
                .MaximumLength(150)
                .When(x => !string.IsNullOrWhiteSpace(x.Filter.FullName));

            RuleFor(x => x.Filter.Email)
                .MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Filter.Email));

            RuleFor(x => x.Filter.SortBy)
                .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || AllowedSortFields.Contains(sortBy.Trim()))
                .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");
        });
    }
}
