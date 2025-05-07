using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Payments
{
    public class EditPaymentRequest
    {
        public required string StripePaymentMethodId { get; set; }
        public required string CardHolderName { get; set; }
    }
}