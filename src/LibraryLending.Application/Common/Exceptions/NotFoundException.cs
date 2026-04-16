namespace LibraryLending.Application.Common.Exceptions;

public abstract class NotFoundException : ApplicationLayerException
{
    protected NotFoundException(string message)
        : base(message)
    {
    }
}
