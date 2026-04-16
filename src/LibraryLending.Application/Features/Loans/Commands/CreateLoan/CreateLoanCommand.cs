using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Features.Loans.Contracts;

namespace LibraryLending.Application.Features.Loans.Commands.CreateLoan;

public sealed record CreateLoanCommand(CreateLoanRequest Request) : ICommand<LoanResponse>;
