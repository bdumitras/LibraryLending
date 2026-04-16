using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Features.Loans.Contracts;

namespace LibraryLending.Application.Features.Loans.Queries.GetLoanById;

public sealed record GetLoanByIdQuery(Guid Id) : IQuery<LoanResponse>;
