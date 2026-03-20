using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Categories.Commands;

public record UpdateCategoriesSectionCommand(
    int ShopId,
    string Name,
    bool Visibility,
    IReadOnlyList<int> CategoryIds) : IRequest;

public class UpdateCategoriesSectionCommandHandler(
    IShopRepository shopRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCategoriesSectionCommand>
{
    public async Task Handle(UpdateCategoriesSectionCommand request, CancellationToken cancellationToken)
    {
        if (request.CategoryIds.Count < 1)
        {
            throw new BadRequestException("At least one category must be selected.");
        }

        if (request.CategoryIds.Count > 8)
        {
            throw new BadRequestException("Maximum 8 categories allowed. Please remove one to add another.");
        }

        var distinctIds = request.CategoryIds.Distinct().ToList();
        if (distinctIds.Count != request.CategoryIds.Count)
        {
            throw new BadRequestException("Duplicate categories are not allowed.");
        }

        var shop = await shopRepository.GetByIdAsync(request.ShopId, cancellationToken);

        if (shop == null)
            throw new NotFoundException($"Shop with id {request.ShopId} not found");

        var requestedCategoriesSpec = new SpecificationBuilder<CategoryEntity>()
            .Add<GetVisibleLevelOneCategoriesByShopIdSpecification>(request.ShopId)
            .Add<GetCategoriesByIdsSpecification>(distinctIds)
            .Build();

        var requestedCategories = (await categoryRepository.GetAllBySpecAsync(requestedCategoriesSpec, cancellationToken)).ToList();
        if (requestedCategories.Count != distinctIds.Count)
        {
            throw new BadRequestException("Selected categories must be level 1, visible, and belong to your shop.");
        }

        var currentSelectionSpec = new SpecificationBuilder<CategoryEntity>()
            .Add<GetCategoriesByShopIdSpecification>(request.ShopId)
            .AddCriteria(c => c.CategoriesSectionOrder != null)
            .Build();

        var currentSelection = (await categoryRepository.GetAllBySpecAsync(currentSelectionSpec, cancellationToken)).ToList();
        var requestedById = requestedCategories.ToDictionary(category => category.Id);

        using var transaction = unitOfWork.BeginTransaction();

        foreach (var category in currentSelection)
        {
            category.CategoriesSectionOrder = null;
        }

        var categoriesToUpdate = currentSelection.ToDictionary(category => category.Id);

        for (var index = 0; index < request.CategoryIds.Count; index++)
        {
            var categoryId = request.CategoryIds[index];
            var category = requestedById[categoryId];

            category.CategoriesSectionOrder = index;
            categoriesToUpdate[category.Id] = category;
        }

        shop.CategoriesSectionName = request.Name.Trim();
        shop.CategoriesSectionVisibility = request.Visibility;

        await categoryRepository.UpdateManyAsync(categoriesToUpdate.Values, cancellationToken);
        await shopRepository.UpdateAsync(shop, cancellationToken);

        transaction.Commit();
    }
}
