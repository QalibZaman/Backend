using Microsoft.AspNetCore.Http;

namespace NettoRetail.Application.Models.Response.ShopDocument
{
    public class GetShopDocumentResponce
    {
        public required string DocumentType { get; set; }
        public required string FileName { get; set; }
        public required string FileType { get; set; }
        public required string FileUrl { get; set; }
    }
}
