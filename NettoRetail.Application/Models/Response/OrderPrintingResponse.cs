using Microsoft.AspNetCore.Http;

namespace NettoRetail.Application.Models.Response;

public class OrderPrintingResponse
{
    public Stream Stream { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; } = "application/pdf";
}
