namespace LibraryLending.UnitTests.Application.Handlers.Members;

using FluentValidation;
using LibraryLending.Application.Features.Members.Commands.UpdateMember;
using LibraryLending.Application.Features.Members.Contracts;
using LibraryLending.Application.Features.Members.Exceptions;
using LibraryLending.Domain.Entities;
using LibraryLending.UnitTests.Application.Handlers.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

public class UpdateMemberCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Update_Member_And_Save_When_Command_Is_Valid()
    {
        var member = new Member("Old Name", "old@example.com", true);
        var memberRepository = new InMemoryMemberRepository(new[] { member });
        var unitOfWork = new RecordingUnitOfWork();
        var validator = new InlineValidator<UpdateMemberCommand>();
        var handler = new UpdateMemberCommandHandler(
            memberRepository,
            unitOfWork,
            validator,
            NullLogger<UpdateMemberCommandHandler>.Instance);

        var command = new UpdateMemberCommand(member.Id, new UpdateMemberRequest
        {
            FullName = " Updated Name ",
            Email = " updated@example.com ",
            IsActive = false
        });

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(member.Id, response.Id);
        Assert.Equal("Updated Name", response.FullName);
        Assert.Equal("updated@example.com", response.Email);
        Assert.False(response.IsActive);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.True(memberRepository.UpdateCalled);
        Assert.Equal("Updated Name", member.FullName);
        Assert.Equal("updated@example.com", member.Email);
        Assert.False(member.IsActive);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Member_Does_Not_Exist()
    {
        var memberRepository = new InMemoryMemberRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var validator = new InlineValidator<UpdateMemberCommand>();
        var handler = new UpdateMemberCommandHandler(
            memberRepository,
            unitOfWork,
            validator,
            NullLogger<UpdateMemberCommandHandler>.Instance);

        var command = new UpdateMemberCommand(Guid.NewGuid(), new UpdateMemberRequest
        {
            FullName = "Updated Name",
            Email = "updated@example.com",
            IsActive = true
        });

        await Assert.ThrowsAsync<MemberNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }
}
