using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Commands;

public record DeleteCategoryCommand(int Id) : IRequest;

public class DeleteCategoryCommandHandler(ICategoryRepository categoryRepository) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetCategoryByIdSpecification(request.Id);

        var category = await categoryRepository.GetAsync(specification, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException($"There is no category with Id: {request.Id}");
        }

        var deleteSpecification = new GetCategoryByIdSpecification(category.Id);

        await categoryRepository.DeleteAsync(deleteSpecification, cancellationToken);
    }
}
