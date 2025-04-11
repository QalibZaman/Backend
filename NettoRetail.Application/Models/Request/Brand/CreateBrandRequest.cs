using Microsoft.AspNetCore.Http;

namespace NettoRetail.Application.Models.Request.Brand;

public class CreateBrandRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string WebsiteUrl { get; set; }
    public IFormFile? LogoFile { get; set; }
}
