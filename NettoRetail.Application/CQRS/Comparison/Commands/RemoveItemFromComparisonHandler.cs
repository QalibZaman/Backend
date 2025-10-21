using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Comparison;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Comparison.Commands;

public record RemoveItemFromComparisonCommand(Guid CustomerId, Guid InventoryItemId) : IRequest;
public class RemoveItemFromComparisonHandler(
IComparisonRepository comparisonRepository) : IRequestHandler<RemoveItemFromComparisonCommand>
{
    public async Task Handle(RemoveItemFromComparisonCommand request, CancellationToken cancellationToken)
    {
        var listSpec = new SpecificationBuilder<ComparisonListEntity>()
            .AddCriteria(list => list.CustomerId == request.CustomerId &&
                     list.ComparisonListItems.Any(i => i.InventoryItemId == request.InventoryItemId))
            .AddInclude(list => list.ComparisonListItems)
            .Build();

        var comparisonList = await comparisonRepository.GetAsync(listSpec, cancellationToken);

        if (comparisonList is null)
            throw new NotFoundException("Comparison list not found.");

        var item = comparisonList.ComparisonListItems
            .FirstOrDefault(i => i.InventoryItemId == request.InventoryItemId);

        if (item is null)
            throw new NotFoundException("Item not found in comparison list.");

        comparisonList.ComparisonListItems.Remove(item);
        await comparisonRepository.UpdateAsync(comparisonList, cancellationToken);
    }
}