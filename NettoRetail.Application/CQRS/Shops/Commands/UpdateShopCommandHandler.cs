using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.Application.Services.IShopValidationService;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record UpdateShopCommand(int Id, ShopModel Model) : IRequest;

public class UpdateShopCommandHandler(IShopRepository shopRepository, ICountryDetailsValidationService countryDetailsValidator, IShopDetailsValidationService shopDetailsValidator) : IRequestHandler<UpdateShopCommand>
{
    public async Task Handle(UpdateShopCommand request, CancellationToken cancellationToken)
    {
        var countryValidationModel = request.Model.Adapt<CountryValidationModel>();

        await countryDetailsValidator.ValidateAsync(countryValidationModel, cancellationToken);

        var shopValidationModel = request.Model.Adapt<ShopValidationModel>();

        await shopDetailsValidator.ValidateShopOnUpdate(shopValidationModel, request.Id, cancellationToken);

        var specification = new GetShopByIdSpecification(request.Id);
        var shopToUpdate = await shopRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException($"There is no shop with such Id");

        request.Model.Adapt(shopToUpdate);
        await shopRepository.UpdateAsync(shopToUpdate, cancellationToken);
    }
}
