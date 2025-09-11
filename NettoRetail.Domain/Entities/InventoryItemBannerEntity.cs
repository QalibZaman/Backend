using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Domain.Entities
{
    public class InventoryItemBannerEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string FileIdentifier { get; set; }
        public int OrderIndex { get; set; }
        public required Guid InventoryItemId { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
    }
}
