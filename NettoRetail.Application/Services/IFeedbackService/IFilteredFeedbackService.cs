using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Feedback;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IFeedbackService;

public interface IFilteredFeedbackService
{
    Specification<FeedbackEntity> BuildFilteredFeedbacksSpecification(GetFilteredFeedbacksModel model, PaginationRequest pagination);
}
