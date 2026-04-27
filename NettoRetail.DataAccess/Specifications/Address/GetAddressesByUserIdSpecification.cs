using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Address;

public class GetAddressesByUserIdSpecification : Specification<AddressEntity>
{
    public GetAddressesByUserIdSpecification(Guid userId)
        : base(x => x.UserId == userId)
    {
        AddInclude(x => x.Country);
    }
}
