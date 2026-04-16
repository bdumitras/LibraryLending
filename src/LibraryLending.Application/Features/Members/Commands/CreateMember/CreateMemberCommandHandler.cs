using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Features.Members.Exceptions;
using LibraryLending.Application.Features.Members.Mappings;
using LibraryLending.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Members.Commands.CreateMember;

public sealed class CreateMemberCommandHandler : ICommandHandler<CreateMemberCommand, Contracts.MemberResponse>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateMemberCommand> _validator;
    private readonly ILogger<CreateMemberCommandHandler> _logger;

    public CreateMemberCommandHandler(
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateMemberCommand> validator,
        ILogger<CreateMemberCommandHandler> logger)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Contracts.MemberResponse> Handle(CreateMemberCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var normalizedEmail = command.Request.Email.Trim();
        var exists = await _memberRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken: cancellationToken);
        if (exists)
        {
            throw new MemberEmailAlreadyExistsException(normalizedEmail);
        }

        var member = new Member(
            command.Request.FullName,
            normalizedEmail,
            command.Request.IsActive);

        await _memberRepository.AddAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogEvents.MemberCreated,
            "Created member {MemberId} with email {Email}. IsActive {IsActive}.",
            member.Id,
            member.Email,
            member.IsActive);

        return member.ToResponse();
    }
}
