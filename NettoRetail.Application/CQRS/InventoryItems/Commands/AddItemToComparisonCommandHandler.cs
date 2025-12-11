    using Mapster;
    using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
    using NettoRetail.Application.Models.Response;
    using NettoRetail.DataAccess.Specifications.Inventory;
    using NettoRetail.Domain.Entities;
    using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;

    namespace NettoRetail.Application.CQRS.InventoryItems.Commands;
    public record AddItemToComparisonCommand(Guid ItemId, Guid UserId) : IRequest<ItemComparisonResponse>;

    public class AddItemToComparisonCommandHandler(IItemComparisonRepository itemComparisonRepository,
        IInventoryItemRepository inventoryItemRepository,
        IOptions<ComparisonOptions> comparisonOptions)
        : IRequestHandler<AddItemToComparisonCommand, ItemComparisonResponse>
    {
        public async Task<ItemComparisonResponse> Handle(AddItemToComparisonCommand request, CancellationToken cancellationToken)
        {
            var item = await inventoryItemRepository.GetAsync(
                new GetItemByIdSpecification(request.ItemId),
                cancellationToken)
                ?? throw new NotFoundException("Item not found");

            var comparisonSpecificationBuilder = new SpecificationBuilder<ItemComparisonEntity>();
            comparisonSpecificationBuilder.Add<GetItemComparisonByUserIdSpecification>(request.UserId);
            comparisonSpecificationBuilder.AddInclude(x => x.ComparisonItems);

            var comparison = await itemComparisonRepository.GetAsync(
                comparisonSpecificationBuilder.Build(),
                cancellationToken);

            ItemComparisonEntity result;

            if (comparison == null)
            {
                result = await itemComparisonRepository.AddComparisonAsync(request.ItemId, request.UserId, cancellationToken);
                return result.Adapt<ItemComparisonResponse>();
            }

            if (comparison.GetItem(request.ItemId) != null)
                throw new BadRequestException("Item already added to comparison");

            if (comparison.ComparisonItems.Count == comparisonOptions.Value.MaxCount)
                throw new BadRequestException("You can compare up to 4 products only");

            comparison.AddItem(request.ItemId);
            result = await itemComparisonRepository.UpdateAsync(comparison, cancellationToken);

            var response = result.Adapt<ItemComparisonResponse>();

            return response;
        }
    }
