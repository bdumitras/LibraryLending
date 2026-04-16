using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Features.Loans.Exceptions;
using LibraryLending.Application.Features.Loans.Mappings;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Loans.Queries.GetLoanById;

public sealed class GetLoanByIdQueryHandler : IQueryHandler<GetLoanByIdQuery, Contracts.LoanResponse>
{
    private readonly ILoanRepository _loanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<GetLoanByIdQuery> _validator;
    private readonly ILogger<GetLoanByIdQueryHandler> _logger;

    public GetLoanByIdQueryHandler(
        ILoanRepository loanRepository,
        IUnitOfWork unitOfWork,
        IValidator<GetLoanByIdQuery> validator,
        ILogger<GetLoanByIdQueryHandler> logger)
    {
        _loanRepository = loanRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Contracts.LoanResponse> Handle(GetLoanByIdQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var loan = await _loanRepository.GetByIdAsync(query.Id, cancellationToken);
        if (loan is null)
        {
            throw new LoanNotFoundException(query.Id);
        }

        var originalStatus = loan.Status;
        loan.RefreshStatus(DateTime.UtcNow);

        if (loan.Status != originalStatus)
        {
            _loanRepository.Update(loan);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                ApplicationLogEvents.LoanStatusesRefreshed,
                "Refreshed loan {LoanId} status from {PreviousStatus} to {CurrentStatus} during retrieval.",
                loan.Id,
                originalStatus,
                loan.Status);
        }

        _logger.LogDebug(
            ApplicationLogEvents.LoanRetrieved,
            "Retrieved loan {LoanId} for book {BookId} and member {MemberId}. Status {Status}.",
            loan.Id,
            loan.BookId,
            loan.MemberId,
            loan.Status);

        return loan.ToResponse();
    }
}
