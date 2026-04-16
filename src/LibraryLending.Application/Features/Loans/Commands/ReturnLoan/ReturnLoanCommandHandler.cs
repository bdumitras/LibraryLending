using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Features.Books.Exceptions;
using LibraryLending.Application.Features.Loans.Exceptions;
using LibraryLending.Application.Features.Loans.Mappings;
using LibraryLending.Domain.Services;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Loans.Commands.ReturnLoan;

public sealed class ReturnLoanCommandHandler : ICommandHandler<ReturnLoanCommand, Contracts.LoanResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LoanDomainService _loanDomainService;
    private readonly IValidator<ReturnLoanCommand> _validator;
    private readonly ILogger<ReturnLoanCommandHandler> _logger;

    public ReturnLoanCommandHandler(
        IBookRepository bookRepository,
        ILoanRepository loanRepository,
        IUnitOfWork unitOfWork,
        LoanDomainService loanDomainService,
        IValidator<ReturnLoanCommand> validator,
        ILogger<ReturnLoanCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _loanRepository = loanRepository;
        _unitOfWork = unitOfWork;
        _loanDomainService = loanDomainService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Contracts.LoanResponse> Handle(ReturnLoanCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var loan = await _loanRepository.GetByIdAsync(command.Id, cancellationToken);
        if (loan is null)
        {
            throw new LoanNotFoundException(command.Id);
        }

        var book = await _bookRepository.GetByIdAsync(loan.BookId, cancellationToken);
        if (book is null)
        {
            throw new BookNotFoundException(loan.BookId);
        }

        _loanDomainService.ReturnLoan(book, loan, command.Request.ReturnedAtUtc);

        _bookRepository.Update(book);
        _loanRepository.Update(loan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogEvents.LoanReturned,
            "Returned loan {LoanId} for book {BookId}. Member {MemberId}. ReturnedAtUtc {ReturnedAtUtc}.",
            loan.Id,
            loan.BookId,
            loan.MemberId,
            loan.ReturnedAtUtc);

        return loan.ToResponse();
    }
}
