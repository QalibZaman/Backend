using NettoRetail.Domain.Entities.Enums;

namespace NettoRetail.Application.Models.Response
{
    public class ShopDocumentResponce
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public ShopDocumentType DocumentType { get; set; }
        public string FileName { get; set; } = null!;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
