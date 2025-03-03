using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Category;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Commands
{
    public record UpdateCategoryCommand(int Id, string Name, string URL, bool Visibility, int? ParentId) : IRequest<CategoryResponse>;

    public class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, ICategoryLevelManagementService levelService) : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
    {
        public async Task<CategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await categoryRepository.GetAsync(new GetCategoryByIdSpecification(request.Id), cancellationToken);

            if (category is null)
            {
                throw new NotFoundException($"There is no category with Id: {category.Id}");
            }

            var newCategory = request.Adapt(category);

            await levelService.UpdateCategoryLevel(newCategory, cancellationToken);

            var response = category.Adapt<CategoryResponse>();

            return response;
        }
    }
}
