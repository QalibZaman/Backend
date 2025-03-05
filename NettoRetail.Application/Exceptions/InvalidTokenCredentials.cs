namespace NettoRetail.Application.Exceptions;

public class InvalidTokenCredentialsException : Exception
{
    public InvalidTokenCredentialsException(string message)
        : base(message) { }
}
