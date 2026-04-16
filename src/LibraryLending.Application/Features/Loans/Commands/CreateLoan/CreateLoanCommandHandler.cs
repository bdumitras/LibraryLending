using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Features.Books.Exceptions;
using LibraryLending.Application.Features.Members.Exceptions;
using LibraryLending.Application.Features.Loans.Mappings;
using LibraryLending.Domain.Services;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Loans.Commands.CreateLoan;

public sealed class CreateLoanCommandHandler : ICommandHandler<CreateLoanCommand, Contracts.LoanResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LoanDomainService _loanDomainService;
    private readonly IValidator<CreateLoanCommand> _validator;
    private readonly ILogger<CreateLoanCommandHandler> _logger;

    public CreateLoanCommandHandler(
        IBookRepository bookRepository,
        IMemberRepository memberRepository,
        ILoanRepository loanRepository,
        IUnitOfWork unitOfWork,
        LoanDomainService loanDomainService,
        IValidator<CreateLoanCommand> validator,
        ILogger<CreateLoanCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _memberRepository = memberRepository;
        _loanRepository = loanRepository;
        _unitOfWork = unitOfWork;
        _loanDomainService = loanDomainService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Contracts.LoanResponse> Handle(CreateLoanCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var book = await _bookRepository.GetByIdAsync(command.Request.BookId, cancellationToken);
        if (book is null)
        {
            throw new BookNotFoundException(command.Request.BookId);
        }

        var member = await _memberRepository.GetByIdAsync(command.Request.MemberId, cancellationToken);
        if (member is null)
        {
            throw new MemberNotFoundException(command.Request.MemberId);
        }

        var loan = _loanDomainService.CreateLoan(
            book,
            member,
            command.Request.LoanDateUtc,
            command.Request.DueDateUtc);

        _bookRepository.Update(book);
        await _loanRepository.AddAsync(loan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogEvents.LoanCreated,
            "Created loan {LoanId} for book {BookId} and member {MemberId}. LoanDateUtc {LoanDateUtc}. DueDateUtc {DueDateUtc}.",
            loan.Id,
            loan.BookId,
            loan.MemberId,
            loan.LoanDateUtc,
            loan.DueDateUtc);

        return loan.ToResponse();
    }
}
