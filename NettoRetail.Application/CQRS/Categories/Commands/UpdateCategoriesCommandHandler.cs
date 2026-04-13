using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.Application.Services.ICategoryService;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using MapsterMapper;
namespace NettoRetail.Application.CQRS.Categories.Commands
{
    public record UpdateCategoriesCommand(int Id, string Name, string? URL, bool Visibility, int? ParentId, int ShopId) : IRequest<CategoryResponse>;

    public class UpdateCategoryCommandHandlers(
        ICategoryRepository categoryRepository,
        ICategoryLevelManagementService levelService,
        ICategoryService categoryService,
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<UpdateCategoriesCommand, CategoryResponse>
    {
        public async Task<CategoryResponse> Handle(UpdateCategoriesCommand request, CancellationToken cancellationToken)
        {
            var specification = new GetCategoryByIdAndShopIdSpecification(request.Id, request.ShopId);

            var category = await categoryRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException($"There is no category with such Id");

            if (request.ParentId == request.Id)
            {
                throw new BadRequestException("Category cannot be its own parent.");
            }

            CategoryEntity? parentCategory = null;
            if (request.ParentId is int parentId)
            {
                parentCategory = await categoryRepository.GetAsync(
                    new GetCategoryByIdAndShopIdSpecification(parentId, request.ShopId),
                    cancellationToken);

                if (parentCategory == null)
                {
                    throw new NotFoundException("Parent category not found");
                }
            }

            var normalizedUrl = categoryService.BuildCategoryUrl(request.Name, request.URL, parentCategory?.URL);

            var getCategoriesByNameOrUrlSpecification = new GetCategoriesByNameOrUrlSpecification(request.Name, normalizedUrl, request.ShopId);

            var conflictModel = await categoryRepository.GetAsync(getCategoriesByNameOrUrlSpecification, cancellationToken);

            if (conflictModel != null && conflictModel.Id != category.Id)
            {
                throw new ConflictException("Category with such name or url already exists");
            }

            //UpdateCategoryLevel recursively updates category levels. It updates categories until it hits MAX_LEVEL, throwing an exception only then. If all updates succeed, commit; if an exception occurs, rollback.
            var transaction = unitOfWork.BeginTransaction();

            try
            {
                mapper.Map(request, category);

                await levelService.UpdateCategoryLevel(category, cancellationToken);

                transaction.Commit();

                var response = mapper.Map<CategoryResponse>(category);

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
