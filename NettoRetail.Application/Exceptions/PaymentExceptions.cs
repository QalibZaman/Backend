namespace NettoRetail.Application.Exceptions;

public class PaymentExceptions : Exception
{
    public PaymentExceptions(string message)
           : base(message) { }
}
