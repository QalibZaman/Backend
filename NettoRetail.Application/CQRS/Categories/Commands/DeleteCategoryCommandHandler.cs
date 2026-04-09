using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ICategoryService;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Commands;

public record DeleteCategoryCommand(int Id, int ShopId) : IRequest;

public class DeleteCategoryCommandHandler(ICategoryService categoryService) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        await categoryService.DeleteCategoryAsync(request.Id, request.ShopId, cancellationToken);
    }
}
