using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.PlatformAdmin;
public class GetPlatformAdminByIdSpecification : Specification<PlatformAdminEntity>
{
    public GetPlatformAdminByIdSpecification(Guid id)
    : base(x => x.AccountId == id) { }
}