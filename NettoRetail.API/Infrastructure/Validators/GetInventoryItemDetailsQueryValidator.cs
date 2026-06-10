using FluentValidation;
using NettoRetail.Application.CQRS.InventoryItems.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class GetInventoryItemDetailsQueryValidator : AbstractValidator<GetInventoryItemDetailsQuery>
    {
        private readonly IInventoryItemRepository _inventoryItemRepository;
        public GetInventoryItemDetailsQueryValidator(IInventoryItemRepository inventoryItemRepository)
        {
            _inventoryItemRepository = inventoryItemRepository;
            ConfigureRules();
        }

        private void ConfigureRules()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .CustomAsync(ValidateItemExists);
        }

        private async Task ValidateItemExists(Guid id, ValidationContext<GetInventoryItemDetailsQuery> context, CancellationToken cancellationToken)
        {
            var itemSpecification = new GetItemByIdSpecification(id);

            var itemExists = await _inventoryItemRepository.Exists(itemSpecification, cancellationToken);

            if (!itemExists)
            {
                context.AddFailure(ValidationErrorMessages.Inventory.ItemNotFoundErrorMessage);
            }
        }
    }
}
