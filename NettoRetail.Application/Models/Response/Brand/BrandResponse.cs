namespace NettoRetail.Application.Models.Response.Brand;

public class BrandResponse
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoImageUrl { get; set; }
}
