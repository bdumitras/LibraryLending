using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Loans.Mappings;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Loans.Queries.GetLoans;

public sealed class GetLoansQueryHandler : IQueryHandler<GetLoansQuery, PagedResponse<Contracts.LoanResponse>>
{
    private readonly ILoanRepository _loanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<GetLoansQuery> _validator;
    private readonly ILogger<GetLoansQueryHandler> _logger;

    public GetLoansQueryHandler(
        ILoanRepository loanRepository,
        IUnitOfWork unitOfWork,
        IValidator<GetLoansQuery> validator,
        ILogger<GetLoansQueryHandler> logger)
    {
        _loanRepository = loanRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<PagedResponse<Contracts.LoanResponse>> Handle(GetLoansQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var pagedLoans = await _loanRepository.GetPagedAsync(query.Filter, cancellationToken);

        var hasChanges = false;
        var refreshedCount = 0;
        var nowUtc = DateTime.UtcNow;

        foreach (var loan in pagedLoans.Items)
        {
            var originalStatus = loan.Status;
            loan.RefreshStatus(nowUtc);
            if (loan.Status != originalStatus)
            {
                _loanRepository.Update(loan);
                hasChanges = true;
                refreshedCount++;
            }
        }

        if (hasChanges)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                ApplicationLogEvents.LoanStatusesRefreshed,
                "Refreshed statuses for {RefreshedLoanCount} loans while listing loans.",
                refreshedCount);
        }

        _logger.LogDebug(
            ApplicationLogEvents.LoansListed,
            "Retrieved loans page {Page} with page size {PageSize}. TotalCount {TotalCount}. SearchTerm {SearchTerm}. BookId {BookId}. MemberId {MemberId}. Status {Status}. SortBy {SortBy}. SortDirection {SortDirection}.",
            pagedLoans.Page,
            pagedLoans.PageSize,
            pagedLoans.TotalCount,
            query.Filter.SearchTerm,
            query.Filter.BookId,
            query.Filter.MemberId,
            query.Filter.Status,
            query.Filter.SortBy,
            query.Filter.SortDirection);

        return pagedLoans.Map(loan => loan.ToResponse());
    }
}
