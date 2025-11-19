using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions;

public class GetSessionByIdSpecification : Specification<SessionEntity>
{
    public GetSessionByIdSpecification(Guid id)
        : base(s => s.Id == id && s.IsActive) { }
}
