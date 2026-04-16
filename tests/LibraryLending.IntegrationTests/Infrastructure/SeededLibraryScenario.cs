namespace LibraryLending.IntegrationTests.Infrastructure;

public sealed record SeededLibraryScenario(
    Guid AvailableBookId,
    Guid LoanedBookId,
    Guid ReturnedBookId,
    Guid ActiveMemberId,
    Guid InactiveMemberId,
    Guid ReturningMemberId,
    Guid ActiveLoanId,
    Guid ReturnedLoanId);
