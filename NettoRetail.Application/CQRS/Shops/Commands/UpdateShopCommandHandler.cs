using MapsterMapper;
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

public class UpdateShopCommandHandler(
    IShopRepository shopRepository,
    ICountryDetailsValidationService countryDetailsValidator,
    IShopValidationService shopDetailsValidator,
    IMapper mapper) : IRequestHandler<UpdateShopCommand>
{
    public async Task Handle(UpdateShopCommand request, CancellationToken cancellationToken)
    {
        if (request.Model.CountryId != null)
        {
            var countryValidationModel = mapper.Map<CountryValidationModel>(request.Model);
            await countryDetailsValidator.ValidateAsync(countryValidationModel, cancellationToken);
        }

        var shopValidationModel = mapper.Map<ShopValidationModel>(request.Model);

        await shopDetailsValidator.ValidateShopOnUpdate(shopValidationModel, request.Id, cancellationToken);

        var specification = new GetShopByIdSpecification(request.Id);
        var shopToUpdate = await shopRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException($"There is no shop with such Id");

        mapper.Map(request.Model, shopToUpdate);
        await shopRepository.UpdateAsync(shopToUpdate, cancellationToken);
    }
}
