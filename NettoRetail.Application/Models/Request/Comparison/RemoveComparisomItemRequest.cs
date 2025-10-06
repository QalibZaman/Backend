using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Request.Comparison
{
    public class RemoveComparisonItemRequest
    {
        public required string UserId { get; set; }
        public required Guid InventoryItemId { get; set; }
    }
}
