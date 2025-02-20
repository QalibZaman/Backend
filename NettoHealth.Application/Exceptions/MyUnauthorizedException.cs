namespace NettoHealth.Application.Exceptions;

public class MyUnauthorizedException : Exception
{
    public MyUnauthorizedException(string message)
        : base(message) { }
}