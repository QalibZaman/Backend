using Mapster;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record DeleteItemFromComparisonCommand(Guid ItemId, Guid UserId) : IRequest<ItemComparisonResponse>;
public class DeleteItemFromComparisonCommandHandler(
    IItemComparisonRepository itemComparisonRepository)
    : IRequestHandler<DeleteItemFromComparisonCommand, ItemComparisonResponse>
{
    public async Task<ItemComparisonResponse> Handle(DeleteItemFromComparisonCommand request, CancellationToken cancellationToken)
    {
        var comparisonSpecificationBuilder = new SpecificationBuilder<ItemComparisonEntity>();
        comparisonSpecificationBuilder.Add<GetItemComparisonByUserIdSpecification>(request.UserId);
        comparisonSpecificationBuilder.AddInclude(x => x.ComparisonItems);

        var comparison = await itemComparisonRepository.GetAsync(
            comparisonSpecificationBuilder.Build(),
            cancellationToken)
            ?? throw new NotFoundException("Comparison not found");

        var itemSpecification = new GetInventoryItemByIdSpecification(request.ItemId);

        var link = comparison.GetItem(request.ItemId)
            ?? throw new NotFoundException("Item is not in comparison");

        var result = await itemComparisonRepository.DeleteItemAsync(comparison, link, cancellationToken);

        var response = result.Adapt<ItemComparisonResponse>();

        return response;
    }
}
