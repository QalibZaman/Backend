namespace NettoRetail.Application.Exceptions;

public class MyUnauthorizedException(string message, DateTime? lockoutEndTime = null) : Exception(message)
{
    public DateTime? LockoutEndTime { get; } = lockoutEndTime;
}