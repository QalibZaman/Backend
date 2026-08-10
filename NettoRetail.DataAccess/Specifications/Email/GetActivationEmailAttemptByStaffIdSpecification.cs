using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Email;

public class GetActivationEmailAttemptByStaffIdSpecification: Specification<ActivationEmailAttemptEntity>
{
    public GetActivationEmailAttemptByStaffIdSpecification(Guid id)
        : base(x => x.StaffId == id) { }
}