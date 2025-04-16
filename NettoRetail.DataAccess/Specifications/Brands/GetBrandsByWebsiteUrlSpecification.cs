using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Brands;

public class GetBrandsByWebsiteUrlSpecification : Specification<BrandEntity>
{
    public GetBrandsByWebsiteUrlSpecification(string websiteUrl)
    : base(b => b.WebsiteUrl == websiteUrl) { }
}
