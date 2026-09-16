using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Category;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Commands;

public record UpdateCategoryCommand(
    int CategoryId,
    UpdateCategoryRequest Model,
    int ShopId) : IRequest<CategoryResponse>;

public class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper)
    : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetAsync(
            new GetCategoryByIdAndShopIdSpecification(
                request.CategoryId,
                request.ShopId),
            cancellationToken)
            ?? throw new NotFoundException("Category not found");

        var normalizedName = request.Model.Name.Trim();

        var nameExists = await categoryRepository.ExistsAsync(
            new GetCategoryByNameAndShopIdExceptIdSpecification(
                normalizedName,
                request.CategoryId,
                request.ShopId),
            cancellationToken);

        if (nameExists)
        {
            throw new ConflictException("Category with this name already exists");
        }

        CategoryEntity? parent = null;
        var level = 0;

        if (request.Model.ParentId.HasValue)
        {
            if (request.Model.ParentId.Value == category.Id)
            {
                throw new BadRequestException("Category cannot be its own parent");
            }

            parent = await categoryRepository.GetAsync(
                new GetCategoryByIdAndShopIdSpecification(
                    request.Model.ParentId.Value,
                    request.ShopId),
                cancellationToken)
                ?? throw new NotFoundException("Parent category not found");

            level = parent.Level + 1;
        }

        category.Name = normalizedName;
        category.ParentId = request.Model.ParentId;
        category.Level = level;

        var updatedCategory = await categoryRepository.UpdateAsync(
            category,
            cancellationToken);

        var response = mapper.Map<CategoryResponse>(updatedCategory);
        response.ParentName = parent?.Name;

        return response;
    }
}