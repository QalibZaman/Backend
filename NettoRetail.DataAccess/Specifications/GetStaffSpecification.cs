using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetStaffSpecification : Specification<StaffEntity>
{
    public GetStaffSpecification()
        : base(_ => true) { }
}