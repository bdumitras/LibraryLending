using FluentValidation;

namespace LibraryLending.Application.Features.Loans.Commands.ReturnLoan;

public sealed class ReturnLoanCommandValidator : AbstractValidator<ReturnLoanCommand>
{
    public ReturnLoanCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Request)
            .NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.ReturnedAtUtc)
                .NotEqual(default(DateTime));
        });
    }
}
