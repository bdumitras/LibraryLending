using FluentValidation;

namespace LibraryLending.Application.Features.Members.Commands.CreateMember;

public sealed class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.FullName)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Request.Email)
                .NotEmpty()
                .MaximumLength(256)
                .EmailAddress();
        });
    }
}
