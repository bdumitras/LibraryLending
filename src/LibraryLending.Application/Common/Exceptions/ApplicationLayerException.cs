namespace LibraryLending.Application.Common.Exceptions;

public abstract class ApplicationLayerException : Exception
{
    protected ApplicationLayerException(string message)
        : base(message)
    {
    }
}
