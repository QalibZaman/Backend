using Microsoft.AspNetCore.Http;

public interface IShopDocumentRequest
{
    IFormFile? PrivacyPolicy { get; }
    IFormFile? RefundPolicy { get; }
    IFormFile? ShippingPolicy { get; }
    IFormFile? TermsAndConditions { get; }
}