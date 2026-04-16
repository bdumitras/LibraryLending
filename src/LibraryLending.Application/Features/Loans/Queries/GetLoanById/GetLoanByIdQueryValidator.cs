using FluentValidation;

namespace LibraryLending.Application.Features.Loans.Queries.GetLoanById;

public sealed class GetLoanByIdQueryValidator : AbstractValidator<GetLoanByIdQuery>
{
    public GetLoanByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
