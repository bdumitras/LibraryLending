using FluentValidation;

namespace LibraryLending.Application.Features.Books.Commands.DeleteBook;

public sealed class DeleteBookCommandValidator : AbstractValidator<DeleteBookCommand>
{
    public DeleteBookCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
