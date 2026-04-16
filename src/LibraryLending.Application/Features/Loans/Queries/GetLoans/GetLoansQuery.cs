using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Loans.Contracts;
using LibraryLending.Application.Features.Loans.Models;

namespace LibraryLending.Application.Features.Loans.Queries.GetLoans;

public sealed record GetLoansQuery(LoanListFilter Filter) : IQuery<PagedResponse<LoanResponse>>;
