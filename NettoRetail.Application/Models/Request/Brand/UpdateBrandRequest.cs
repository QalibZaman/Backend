using Microsoft.AspNetCore.Http;

namespace NettoRetail.Application.Models.Request.Brand;

public class UpdateBrandRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public IFormFile? Logo { get; set; }
    public string? WebsiteUrl { get; set; }
}