using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.TeaserSection;

public class GetTeaserSectionByUrlExcludingCurrentSpecification : Specification<TeaserSectionEntity>
{
    public GetTeaserSectionByUrlExcludingCurrentSpecification(string url, Guid id)
        : base (t => t.Link == url && t.Id != id) { }
}
