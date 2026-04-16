using FluentValidation;

namespace LibraryLending.Application.Features.Loans.Commands.CreateLoan;

public sealed class CreateLoanCommandValidator : AbstractValidator<CreateLoanCommand>
{
    public CreateLoanCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.BookId)
                .NotEmpty();

            RuleFor(x => x.Request.MemberId)
                .NotEmpty();

            RuleFor(x => x.Request.LoanDateUtc)
                .NotEqual(default(DateTime));

            RuleFor(x => x.Request.DueDateUtc)
                .NotEqual(default(DateTime));

            RuleFor(x => x.Request)
                .Must(request => request.DueDateUtc > request.LoanDateUtc)
                .WithMessage("DueDateUtc must be greater than LoanDateUtc.");
        });
    }
}
