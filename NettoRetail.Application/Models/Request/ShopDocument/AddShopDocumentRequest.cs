using Microsoft.AspNetCore.Http;

namespace NettoRetail.Application.Models.Request.ShopDocument
{
    public class AddShopDocumentRequest
    {
        public IFormFile? RefundPolicy { get; set; }
        public IFormFile? ShippingPolicy { get; set; }

        public IFormFile? TermsAndConditions { get; set; }

        public IFormFile? PrivacyPolicy { get; set; }
    }
}
