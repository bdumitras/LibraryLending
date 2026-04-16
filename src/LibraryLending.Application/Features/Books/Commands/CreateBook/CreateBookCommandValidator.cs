using FluentValidation;

namespace LibraryLending.Application.Features.Books.Commands.CreateBook;

public sealed class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    private static readonly int MaxPublicationYear = DateTime.UtcNow.Year + 1;

    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Request.Author)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Request.Isbn)
                .NotEmpty()
                .MaximumLength(32);

            RuleFor(x => x.Request.PublicationYear)
                .InclusiveBetween(1450, MaxPublicationYear);

            RuleFor(x => x.Request.TotalCopies)
                .GreaterThan(0);

            RuleFor(x => x.Request.AvailableCopies)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Request)
                .Must(request => request.AvailableCopies <= request.TotalCopies)
                .WithMessage("Available copies cannot exceed total copies.");
        });
    }
}
