using MediatR;
using NettoRetail.Application.Interfaces;

namespace NettoRetail.Application.CQRS.Categories.Commands;

public record UpdateCategoriesSectionCommand(
    int ShopId,
    string Name,
    bool Visibility,
    IReadOnlyList<int> CategoryIds) : IRequest;

public class UpdateCategoriesSectionCommandHandler(
    ICategoriesSectionService categoriesSectionService)
    : IRequestHandler<UpdateCategoriesSectionCommand>
{
    public async Task Handle(UpdateCategoriesSectionCommand request, CancellationToken cancellationToken)
    {
        await categoriesSectionService.UpdateCategoriesSectionAsync(
            request.ShopId,
            request.Name,
            request.Visibility,
            request.CategoryIds,
            cancellationToken);
    }
}
