namespace NettoRetail.Infrastructure.Options;

public class EmailOptions
{
    public required string Sender { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Host { get; set; }
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
}
