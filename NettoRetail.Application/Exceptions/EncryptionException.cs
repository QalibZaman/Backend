namespace NettoRetail.Application.Exceptions;
public class EncryptionException : Exception
{
    public EncryptionException(string message)
        : base(message) { }
}
