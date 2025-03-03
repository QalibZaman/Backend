using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Category;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Categories.Commands
{
    public record UpdateCategoryCommand(int Id, string Name, string URL, bool Visibility, int? ParentId) : IRequest<CategoryResponse>;

    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryLevelManagementService _levelService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, ICategoryLevelManagementService levelService, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _levelService = levelService;
            _unitOfWork = unitOfWork;
        }

        public async Task<CategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetAsync(new GetCategoryByIdSpecification(request.Id), cancellationToken);

            if (category is null)
            {
                throw new NotFoundException($"There is no category with Id: {category.Id}");
            }

            var newCategory = request.Adapt(category);

            var transaction = _unitOfWork.BeginTransaction();

            try
            {
                await _levelService.UpdateCategoryLevel(newCategory, cancellationToken);

                transaction.Commit();

                var response = category.Adapt<CategoryResponse>();

                return response;
            }
            catch (Exception exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
