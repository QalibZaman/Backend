namespace NettoRetail.Infrastructure.Models;

public class BulkEmailModel
{
    public IEnumerable<string> EmailAddresses { get; set; } = new List<string>();
    public string? Subject { get; set; }
    public string? Message { get; set; }
}
