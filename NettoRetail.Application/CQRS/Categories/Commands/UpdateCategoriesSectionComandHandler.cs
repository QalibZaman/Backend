using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using System.ComponentModel.DataAnnotations;

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

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Name is required");

        shop.CategoriesSectionName = request.Name.Trim();
        shop.CategoriesSectionPreview = request.Preview.Trim();
        shop.CategoriesSectionVisibility = request.Visibility;

        await shopRepository.UpdateAsync(shop, cancellationToken);
    }
}