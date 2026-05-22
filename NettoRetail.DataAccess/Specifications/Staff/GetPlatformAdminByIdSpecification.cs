using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;

public class GetPlatformAdminByIdSpecification : Specification<StaffEntity>
{
    public GetPlatformAdminByIdSpecification(Guid id)
        : base(x => x.Id == id && x.Role.Name.StartsWith("PlatformAdmin"))
    {
        AddInclude(x => x.Role);
    }
}
