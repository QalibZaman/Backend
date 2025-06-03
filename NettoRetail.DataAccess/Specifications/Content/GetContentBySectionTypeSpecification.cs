using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Content;

public class GetContentBySectionTypeSpecification : Specification<ContentEntity>
{
    public GetContentBySectionTypeSpecification(SectionTypeEnum sectionType)
        : base(c => c.SectionType == sectionType)
    {
    }
}