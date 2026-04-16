using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Features.Loans.Contracts;

namespace LibraryLending.Application.Features.Loans.Commands.ReturnLoan;

public sealed record ReturnLoanCommand(Guid Id, ReturnLoanRequest Request) : ICommand<LoanResponse>;
