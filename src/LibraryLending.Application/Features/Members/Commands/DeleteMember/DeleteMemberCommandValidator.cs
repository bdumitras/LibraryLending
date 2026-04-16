using FluentValidation;

namespace LibraryLending.Application.Features.Members.Commands.DeleteMember;

public sealed class DeleteMemberCommandValidator : AbstractValidator<DeleteMemberCommand>
{
    public DeleteMemberCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
