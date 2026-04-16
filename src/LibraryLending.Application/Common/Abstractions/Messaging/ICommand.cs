namespace LibraryLending.Application.Common.Abstractions.Messaging;

public interface ICommand<out TResponse>
{
}

public interface ICommand : ICommand<Unit>
{
}
