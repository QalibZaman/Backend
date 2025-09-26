using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.OnboardingSlides;

public class GetOnboardingSlideByIdSpecification : Specification<OnboardingSlideEntity>
{
    public GetOnboardingSlideByIdSpecification(int id)
        : base(x => x.Id == id)
    {
    }
}
