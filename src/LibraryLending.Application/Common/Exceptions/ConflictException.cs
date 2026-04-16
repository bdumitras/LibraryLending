namespace LibraryLending.Application.Common.Exceptions;

public abstract class ConflictException : ApplicationLayerException
{
    protected ConflictException(string message)
        : base(message)
    {
    }
}
