using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Categories.Commands
{
    public record UpdateCategoryCommand(int Id, string Name, string URL, bool Visibility, int? ParentId, int ShopId) : IRequest<CategoryResponse>;

    public class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, ICategoryLevelManagementService levelService, IUnitOfWork unitOfWork) : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
    {
        public async Task<CategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var categorySpec = new GetCategoryByIdAndShopIdSpecification(request.Id, request.ShopId);
            var category = await categoryRepository.GetAsync(categorySpec, cancellationToken)
                ?? throw new NotFoundException("Category not found");

            var normalizedName = request.Name.Trim();
            var normalizedUrl = request.URL.Trim();

            var nameChanged = !string.Equals(category.Name, normalizedName, StringComparison.OrdinalIgnoreCase);
            var urlChanged = !string.Equals(category.URL, normalizedUrl, StringComparison.OrdinalIgnoreCase);
            var visibilityChanged = category.Visibility != request.Visibility;
            var parentChanged = category.ParentId != request.ParentId;

            if (!nameChanged && !urlChanged && !visibilityChanged && !parentChanged)
            {
                return category.Adapt<CategoryResponse>();
            }

            if (parentChanged && request.ParentId is int parentId)
            {
                if (parentId == request.Id)
                {
                    throw new BadRequestException("Category cannot be its own parent.");
                }

                var parentExists = await categoryRepository.Exists(
                    new GetCategoryByIdAndShopIdSpecification(parentId, request.ShopId),
                    cancellationToken);

                if (!parentExists)
                {
                    throw new NotFoundException("Parent category not found");
                }

                var subtree = await categoryRepository.GetCategorySubtreeAsync(request.Id, cancellationToken);
                if (subtree.Any(c => c.Id == parentId))
                {
                    throw new BadRequestException("Category cannot be assigned to its own descendant.");
                }
            }

            var nameConflict = false;
            var urlConflict = false;

            if (nameChanged || urlChanged)
            {
                var conflictSpec = new GetCategoriesByNameOrUrlSpecification(
                    nameChanged ? normalizedName : null,
                    urlChanged ? normalizedUrl : null,
                    request.ShopId,
                    request.Id);

                var conflicts = await categoryRepository.GetManyBySpecAsync(conflictSpec, cancellationToken);
                var conflictList = conflicts as ICollection<CategoryEntity> ?? conflicts.ToList();

                nameConflict = nameChanged && conflictList.Any(c =>
                    string.Equals(c.Name, normalizedName, StringComparison.OrdinalIgnoreCase));

                urlConflict = urlChanged && conflictList.Any(c =>
                    string.Equals(c.URL, normalizedUrl, StringComparison.OrdinalIgnoreCase));
            }

            string? conflictMessage = null;
            if (nameConflict && urlConflict)
            {
                conflictMessage = "Category name already exists (EM-17a). Category URL already exists (EM-17b).";
            }
            else if (nameConflict)
            {
                conflictMessage = "Category name already exists (EM-17a).";
            }
            else if (urlConflict)
            {
                conflictMessage = "Category URL already exists (EM-17b).";
            }

            if (conflictMessage != null)
            {
                throw new ConflictException(conflictMessage);
            }

            //UpdateCategoryLevel recursively updates category levels. It updates categories until it hits MAX_LEVEL, throwing an exception only then. If all updates succeed, commit; if an exception occurs, rollback.
            using var transaction = unitOfWork.BeginTransaction();

            try
            {
                category.Name = normalizedName;
                category.URL = normalizedUrl;
                category.Visibility = request.Visibility;
                category.ParentId = request.ParentId;

                await levelService.UpdateCategoryLevel(category, cancellationToken);

                transaction.Commit();

                var response = category.Adapt<CategoryResponse>();

                return response;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
