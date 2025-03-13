using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Commands;
public record AddCategoryCommand(string Name, string URL, bool Visibility, int? ParentId) : IRequest<CategoryResponse>;

public class AddCategoryCommandHandler(ICategoryRepository categoryRepository, ICategoryLevelManagementService levelService) : IRequestHandler<AddCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
    {
        var getCategoriesByNameOrUrlSpecification = new GetCategoriesByNameOrUrlSpecification(request.Name, request.URL);

        var conflictModel = await categoryRepository.GetAsync(getCategoriesByNameOrUrlSpecification, cancellationToken);

        if (conflictModel != null)
        {
            throw new ConflictException("Category with such name or url is already exist");
        }

        var category = request.Adapt<CategoryEntity>();

        await levelService.SetUpCategoryLevel(category, cancellationToken);

        var responseModel = await categoryRepository.AddAsync(category, cancellationToken);

        var response = responseModel.Adapt<CategoryResponse>();

        return response;
    }
}
