using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response.Comparison;
using NettoRetail.DataAccess.Specifications.Comparison;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Comparison.Queries;

public record GetComparisonListQuery(string UserId, int CategoryId)
    : IRequest<IReadOnlyList<ComparisonItemResponse>>;

public class GetComparisonListQueryHandler(IComparisonRepository comparisonRepository)
    : IRequestHandler<GetComparisonListQuery, IReadOnlyList<ComparisonItemResponse>>
{
    public async Task<IReadOnlyList<ComparisonItemResponse>> Handle(GetComparisonListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var spec = new ComparisonListByUserAndCategorySpec(request.UserId, request.CategoryId, includeItems: true);
            var comparisonList = await comparisonRepository.GetAsync(spec, cancellationToken);

            if (comparisonList?.ComparisonListItems is null || comparisonList.ComparisonListItems.Count == 0)
                return Array.Empty<ComparisonItemResponse>();

            var result = comparisonList.ComparisonListItems
                .OrderByDescending(i => i.CreatedAtUtc)
                .Select(i => i.Adapt<ComparisonItemResponse>())
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            throw new NotFoundException(ex.Message);
        }
    }
}
