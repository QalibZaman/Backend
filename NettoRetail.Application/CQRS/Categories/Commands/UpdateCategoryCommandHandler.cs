using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Categories.Commands
{
    public record UpdateCategoryCommand(int Id, string Name, string URL, bool Visibility, int? ParentId, int ShopId) : IRequest<CategoryResponse>;

    public class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, ICategoryLevelManagementService levelService, IUnitOfWork unitOfWork) : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
    {
        public async Task<CategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var specification = new GetCategoryByIdSpecification(request.Id);

            var category = await categoryRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException($"There is no category with such Id");

            var getCategoriesByNameOrUrlSpecification = new GetCategoriesByNameOrUrlSpecification(request.Name, request.URL, request.ShopId);

            var conflictModel = await categoryRepository.GetAsync(getCategoriesByNameOrUrlSpecification, cancellationToken);

            if (conflictModel != null)
            {
                throw new ConflictException("Category with such name or url already exists");
            }

            //UpdateCategoryLevel recursively updates category levels. It updates categories until it hits MAX_LEVEL, throwing an exception only then. If all updates succeed, commit; if an exception occurs, rollback.
            var transaction = unitOfWork.BeginTransaction();

            try
            {
                request.Adapt(category);

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
