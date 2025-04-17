using System;
using Microsoft.AspNetCore.Http;

public class UpdateBrandRequest
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public IFormFile? Logo { get; set; }
    public required string WebsiteUrl { get; set; }
}
