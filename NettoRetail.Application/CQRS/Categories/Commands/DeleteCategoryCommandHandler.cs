using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Commands;

public record DeleteCategoryCommand(int Id) : IRequest;

public class DeleteCategoryCommandHandler(ICategoryRepository categoryRepository) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetAsync(new GetCategoryByIdSpecification(request.Id), cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"There is no category with Id: {request.Id}");
        }

        await categoryRepository.DeleteAsync(category, cancellationToken);
    }
}
