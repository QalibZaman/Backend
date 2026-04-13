using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;

public class GetCommunicationPreferencesByAccountIdSpecification : Specification<CommunicationPreferenceEntity>
{
    public GetCommunicationPreferencesByAccountIdSpecification(Guid userId) : base(x => x.UserId == userId)
    { }
}
