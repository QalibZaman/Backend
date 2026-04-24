using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Address
{
    public class GetAddressByIdAndUserIdSpecification : Specification<AddressEntity>
    {
        public GetAddressByIdAndUserIdSpecification(Guid id, Guid userId)
            : base(x => x.Id == id && x.UserId == userId)
        {
            AddInclude(x => x.User.Addresses);
            AddInclude(x => x.Country);
        }
    }
}
