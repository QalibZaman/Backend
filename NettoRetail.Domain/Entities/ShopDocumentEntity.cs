using NettoRetail.Domain.Entities.Enums;

namespace NettoRetail.Domain.Entities
{
    public class ShopDocumentEntity
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public ShopDocumentType DocumentType { get; set; }
        public string FileName { get; set; } = null!;
        public long FileSize { get; set; }
        public string BlobName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public ShopEntity Shop { get; set; } = null!;
    }
}
