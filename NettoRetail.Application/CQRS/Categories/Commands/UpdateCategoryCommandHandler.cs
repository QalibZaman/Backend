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

            if (category.Name == normalizedName && category.URL == normalizedUrl &&
                category.Visibility == request.Visibility && category.ParentId == request.ParentId)
            {
                return category.Adapt<CategoryResponse>();
            }

            using var transaction = unitOfWork.BeginTransaction();

            category.Name = normalizedName;
            category.URL = normalizedUrl;
            category.Visibility = request.Visibility;
            category.ParentId = request.ParentId;

            await levelService.UpdateCategoryLevel(category, cancellationToken);

            transaction.Commit();

            return category.Adapt<CategoryResponse>();
        }
    }
}
