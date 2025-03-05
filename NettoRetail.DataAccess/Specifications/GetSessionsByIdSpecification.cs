using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;
public class GetSessionsByIdSpecification : Specification<SessionEntity>
{
    public GetSessionsByIdSpecification(string id)
    : base(x => x.AccountId.ToString() == id) { }
}