using FluentValidation;

namespace LibraryLending.Application.Features.Members.Commands.UpdateMember;

public sealed class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

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
