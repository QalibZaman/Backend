using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Request
{
    public class CreateProductReviewRequest
    {
        public required int Rating { get; set; }
        public required string Title { get; set; }
        public string? ReviewText { get; set; }
        public string? VariantInfo { get; set; }
        public List<string> PhotoUrls { get; set; }
    }
}
