namespace NettoRetail.Application.Models.Response.Brand;

public class BrandResponse
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string WebsiteUrl { get; set; }
    public string? LogoImageUrl { get; set; }
}
