namespace Optivem.Shop.Backend.Core.Exceptions;

public class NotExistValidationException : ValidationException
{
    public NotExistValidationException(string message) : base(message)
    {
    }
}
