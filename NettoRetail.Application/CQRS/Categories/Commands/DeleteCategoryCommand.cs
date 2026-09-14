using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Categories.Commands;

public record DeleteCategoryCommand(int Id, int ShopId) : IRequest;

public class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetAsync(
            new GetCategoryByIdAndShopIdSpecification(request.Id, request.ShopId),
            cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category not found");
        }

        var totalCategoriesInShop = await categoryRepository.GetAmountBySpecAsync(
            new GetCategoriesByShopIdSpecification(request.ShopId),
            cancellationToken);

        if (totalCategoriesInShop == 1)
        {
            throw new BadRequestException("You cannot delete the category because it is the only category in the shop");
        }

        var directChildren = (await categoryRepository.GetAllBySpecAsync(
            new GetCategoriesOfParentByIdSpecification(request.Id),
            cancellationToken)).ToList();

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            if (directChildren.Count > 0)
            {
                foreach (var child in directChildren)
                {
                    child.ParentId = category.ParentId;
                }

                await categoryRepository.UpdateManyAsync(directChildren, cancellationToken);
            }

            await categoryRepository.DeleteAsync(category, cancellationToken);

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}