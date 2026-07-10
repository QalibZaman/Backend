namespace NettoRetail.Infrastructure.Models;

public class EmailModel
{
    public required string Email { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
}
