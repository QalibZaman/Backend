using Microsoft.AspNetCore.Http;

namespace NettoRetail.Application.Models.Request.Brand;

public class CreateBrandRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public IFormFile? LogoFile { get; set; }
}
