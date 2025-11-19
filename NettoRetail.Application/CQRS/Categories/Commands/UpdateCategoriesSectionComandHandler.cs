using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Commands;

public record UpdateCategoriesSectionCommand(
    int ShopId,
    string Name,
    string Preview,
    bool Visibility) : IRequest;

public class UpdateCategoriesSectionCommandHandler(
    IShopRepository shopRepository)
    : IRequestHandler<UpdateCategoriesSectionCommand>
{
    public async Task Handle(UpdateCategoriesSectionCommand request, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetByIdAsync(request.ShopId, cancellationToken);

        if (shop == null)
            throw new NotFoundException($"Shop with id {request.ShopId} not found");

        shop.CategoriesSectionName = request.Name.Trim();
        shop.CategoriesSectionPreview = request.Preview.Trim();
        shop.CategoriesSectionVisibility = request.Visibility;

        await shopRepository.UpdateAsync(shop, cancellationToken);
    }
}