using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Payment
{
    public class ActivePaymentMethodsByUserIdSpecification : Specification<PaymentMethodEntity>
    {
        public ActivePaymentMethodsByUserIdSpecification(Guid userId)
        : base(x => x.AccountId == userId && x.IsActive)
        {
        }
    }
}
