using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Response
{
    public class GetInventoryItemBannerResponse
    {
        public Guid BannerId { get; set; }
        public int OrderIndex { get; set; }
        public string? FileName { get; set; }
        public string FileUrl { get; set; } = null!;
    }
}
