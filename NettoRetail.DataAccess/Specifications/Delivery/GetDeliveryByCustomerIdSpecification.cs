using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Delivery;

public class GetDeliveryByCustomerIdSpecification : Specification<DeliveryEntity>
{
    public GetDeliveryByCustomerIdSpecification(Guid customerId)
        : base(x => x.CustomerId == customerId)
    {
        AddInclude(x => x.Address);
    }
}
