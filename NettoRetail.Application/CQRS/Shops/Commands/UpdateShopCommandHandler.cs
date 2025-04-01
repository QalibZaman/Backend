using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record UpdateShopCommand(int Id, ShopModel Model) : IRequest;

public class UpdateShopCommandHandler(IShopRepository shopRepository, ICountryDetailsValidationService countryDetailsValidator) : IRequestHandler<UpdateShopCommand>
{
    public async Task Handle(UpdateShopCommand request, CancellationToken cancellationToken)
    {
        await countryDetailsValidator.ValidateAsync(request.Model, cancellationToken);

        var specification = new GetShopByIdSpecification(request.Id);
        var shopToUpdate = await shopRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException($"There is no shop with such Id");

        request.Model.Adapt(shopToUpdate);
        await shopRepository.UpdateAsync(shopToUpdate, cancellationToken);
    }
}
