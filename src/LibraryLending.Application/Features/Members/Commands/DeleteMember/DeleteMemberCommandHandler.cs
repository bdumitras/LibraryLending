using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Features.Members.Exceptions;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Members.Commands.DeleteMember;

public sealed class DeleteMemberCommandHandler : ICommandHandler<DeleteMemberCommand>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DeleteMemberCommand> _validator;
    private readonly ILogger<DeleteMemberCommandHandler> _logger;

    public DeleteMemberCommandHandler(
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork,
        IValidator<DeleteMemberCommand> validator,
        ILogger<DeleteMemberCommandHandler> logger)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task Handle(DeleteMemberCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var member = await _memberRepository.GetByIdAsync(command.Id, cancellationToken);
        if (member is null)
        {
            throw new MemberNotFoundException(command.Id);
        }

        _memberRepository.Remove(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogEvents.MemberDeleted,
            "Deleted member {MemberId} with email {Email}.",
            member.Id,
            member.Email);
    }
}
