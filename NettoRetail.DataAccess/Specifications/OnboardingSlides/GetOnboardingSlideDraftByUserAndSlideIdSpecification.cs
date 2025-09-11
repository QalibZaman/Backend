using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.OnboardingSlides;

public class GetOnboardingSlideDraftByUserAndSlideIdSpecification : Specification<OnboardingSlideDraftEntity>
{
    public GetOnboardingSlideDraftByUserAndSlideIdSpecification(Guid userId, int? slideId)
        : base(x => x.UserId == userId && x.SlideId == slideId)
    {
    }
}