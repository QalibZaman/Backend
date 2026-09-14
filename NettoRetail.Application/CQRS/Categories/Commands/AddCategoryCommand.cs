using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Category;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.Application.Services.ICategoryService;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using Pipelines.Sockets.Unofficial.Arenas;

namespace NettoRetail.Application.CQRS.Categories.Commands;

public record AddCategoryCommand(AddCategoryRequest Model, int ShopId) : IRequest<CategoryResponse>;

public class AddCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ICategoryUrlService categoryUrlService,
    IMapper mapper) : IRequestHandler<AddCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Model.Name.Trim();

        var nameExists = await categoryRepository.ExistsAsync(
            new GetCategoryByNameSpecification(normalizedName, request.ShopId),
            cancellationToken);

        if (nameExists)
        {
            throw new ConflictException("Category with this name already exists");
        }

        var level = 0;
        CategoryEntity? parent = null;

        if (request.Model.ParentId.HasValue)
        {
            parent = await categoryRepository.GetAsync(
                new GetCategoryByIdAndShopIdSpecification(request.Model.ParentId.Value, request.ShopId),
                cancellationToken) ?? throw new NotFoundException("Parent category not found");
            level = parent.Level + 1;
        }

        var url = await categoryUrlService.GenerateUniqueUrlAsync(normalizedName, request.ShopId, cancellationToken);

        var category = new CategoryEntity
        {
            Name = normalizedName,
            URL = url,
            Visibility = request.Model.Visibility,
            ParentId = request.Model.ParentId,
            Level = level,
            ShopId = request.ShopId,
            CreatedAt = DateTime.UtcNow
        };

        var savedCategory = await categoryRepository.AddAsync(category, cancellationToken);

        var response = mapper.Map<CategoryResponse>(savedCategory);
        response.ParentName = parent?.Name;

        return response;
    }
}