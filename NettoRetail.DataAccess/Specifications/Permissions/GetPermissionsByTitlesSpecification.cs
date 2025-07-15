using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Permissions
{
    public class GetPermissionsByTitlesSpecification : Specification<PermissionEntity>
    {
        public GetPermissionsByTitlesSpecification(IEnumerable<string> titles)
            : base(p => titles.Contains(p.Title))
        {
        }
    }
}
