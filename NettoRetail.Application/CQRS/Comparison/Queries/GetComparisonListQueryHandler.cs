using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response.Comparison;
using NettoRetail.DataAccess.Specifications.Comparison;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Comparison.Queries;

public record GetComparisonListQuery(Guid CustomerId, int CategoryId)
    : IRequest<IReadOnlyList<ComparisonItemResponse>>;

public class GetComparisonListQueryHandler(IComparisonRepository comparisonRepository)
    : IRequestHandler<GetComparisonListQuery, IReadOnlyList<ComparisonItemResponse>>
{
    public async Task<IReadOnlyList<ComparisonItemResponse>> Handle(GetComparisonListQuery request, CancellationToken cancellationToken)
    {
        var spec = new ComparisonListByUserAndCategorySpec(request.CustomerId, request.CategoryId, includeItems: true);
        var comparisonList = await comparisonRepository.GetAsync(spec, cancellationToken);

        if (comparisonList?.ComparisonListItems is null || comparisonList.ComparisonListItems.Count == 0)
            return Array.Empty<ComparisonItemResponse>();

        var result = comparisonList.ComparisonListItems
            .OrderByDescending(i => i.CreatedAtUtc)
            .Select(i => i.Adapt<ComparisonItemResponse>())
            .ToList();

        return result;
    }
}