using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.Application.Services.IShopValidationService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record CreateShopCommand(Guid AccountId, ShopModel Model) : IRequest<ShopResponse>;

public class CreateShopCommandHandler(
    IShopRepository shopRepository,
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    ICountryDetailsValidationService countryDetailsValidator,
    IShopValidationService shopValidator)
    : IRequestHandler<CreateShopCommand, ShopResponse>
{
    public async Task<ShopResponse> Handle(CreateShopCommand request, CancellationToken cancellationToken)
    {
        if (request.Model.CountryId != null)
        {
            var countryValidationModel = request.Model.Adapt<CountryValidationModel>();
            await countryDetailsValidator.ValidateAsync(countryValidationModel, cancellationToken);
        }

        var shop = request.Model.Adapt<ShopEntity>();
        shop.SubscriptionPlanId = SubscriptionConstants.Free;

        var specification = new GetAccountByIdSpecification(request.AccountId);
        var accountToUpdate = await accountRepository.GetAsync(specification, cancellationToken) ??
            throw new NotFoundException($"Staff member with ID {request.AccountId} not found.");

        if (accountToUpdate.ShopId != null)
        {
            throw new BadRequestException($"Account already has a shop");
        }

        var shopValidationModel = request.Model.Adapt<ShopValidationModel>();
        await shopValidator.ValidateShopOnCreate(shopValidationModel, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();

        var returnedShop = await shopRepository.AddAsync(shop, cancellationToken);

        accountToUpdate.ShopId = returnedShop.Id;
        await accountRepository.UpdateAsync(accountToUpdate, cancellationToken);

        transaction.Commit();

        return returnedShop.Adapt<ShopResponse>();
    }
}
