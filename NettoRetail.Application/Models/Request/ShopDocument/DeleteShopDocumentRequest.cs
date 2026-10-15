using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Request.ShopDocument
{
    public class DeleteShopDocumentRequest
    {
        public bool DeleteRefundPolicy { get; set; } = false;
        public bool DeleteShippingPolicy { get; set; } = false;
        public bool DeleteTermsAndConditions { get; set; } = false;
        public bool DeletePrivacyPolicy { get; set; } = false;
    }
}
