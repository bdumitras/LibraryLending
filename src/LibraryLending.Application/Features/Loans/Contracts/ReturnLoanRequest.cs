namespace LibraryLending.Application.Features.Loans.Contracts;

public sealed record ReturnLoanRequest
{
    public required DateTime ReturnedAtUtc { get; init; }
}
