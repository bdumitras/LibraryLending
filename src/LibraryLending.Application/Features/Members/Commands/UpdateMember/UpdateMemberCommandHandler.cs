using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Features.Members.Exceptions;
using LibraryLending.Application.Features.Members.Mappings;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Members.Commands.UpdateMember;

public sealed class UpdateMemberCommandHandler : ICommandHandler<UpdateMemberCommand, Contracts.MemberResponse>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateMemberCommand> _validator;
    private readonly ILogger<UpdateMemberCommandHandler> _logger;

    public UpdateMemberCommandHandler(
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateMemberCommand> validator,
        ILogger<UpdateMemberCommandHandler> logger)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Contracts.MemberResponse> Handle(UpdateMemberCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var member = await _memberRepository.GetByIdAsync(command.Id, cancellationToken);
        if (member is null)
        {
            throw new MemberNotFoundException(command.Id);
        }

        var normalizedEmail = command.Request.Email.Trim();
        var duplicateEmail = await _memberRepository.ExistsByEmailAsync(normalizedEmail, command.Id, cancellationToken);
        if (duplicateEmail)
        {
            throw new MemberEmailAlreadyExistsException(normalizedEmail);
        }

        member.UpdateDetails(
            command.Request.FullName,
            normalizedEmail,
            command.Request.IsActive);

        _memberRepository.Update(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogEvents.MemberUpdated,
            "Updated member {MemberId} with email {Email}. IsActive {IsActive}.",
            member.Id,
            member.Email,
            member.IsActive);

        return member.ToResponse();
    }
}
