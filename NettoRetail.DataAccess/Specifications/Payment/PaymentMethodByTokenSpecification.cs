using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Payment
{
    public class PaymentMethodByTokenSpecification : Specification<PaymentMethodEntity>
    {
        public PaymentMethodByTokenSpecification(string token)
           : base(x => x.StripePaymentMethodId == token)
        {
        }
    }
}
