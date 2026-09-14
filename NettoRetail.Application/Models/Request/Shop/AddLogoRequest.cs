using Microsoft.AspNetCore.Http;

namespace NettoRetail.Application.Models.Request.Shop;

public class AddLogoRequest
{
    public required IFormFile Logo { get; set; }
}