using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.Application.Services.IShopValidationService;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record CreateShopCommand(Guid AccountId, ShopModel Model) : IRequest<ShopResponse>;

public class CreateShopCommandHandler(
    IShopRepository shopRepository,
    IStaffRepository accountRepository,
    IUnitOfWork unitOfWork,
    ICountryDetailsValidationService countryDetailsValidator,
    IShopValidationService shopValidator,
    IMapper mapper)
    : IRequestHandler<CreateShopCommand, ShopResponse>
{
    public async Task<ShopResponse> Handle(CreateShopCommand request, CancellationToken cancellationToken)
    {
        if (request.Model.CountryId != null)
        {
            var countryValidationModel = mapper.Map<CountryValidationModel>(request.Model);
            await countryDetailsValidator.ValidateAsync(countryValidationModel, cancellationToken);
        }

        var shop = mapper.Map<ShopEntity>(request.Model);
        shop.SubscriptionPlanId = SubscriptionConstants.Free;

        var specification = new GetStaffByIdSpecification(request.AccountId);
        var accountToUpdate = await accountRepository.GetAsync(specification, cancellationToken) ??
            throw new NotFoundException($"Staff member with ID {request.AccountId} not found.");

        if (accountToUpdate.ShopId != null)
        {
            throw new BadRequestException($"Account already has a shop");
        }

        var shopValidationModel = mapper.Map<ShopValidationModel>(request.Model);
        await shopValidator.ValidateShopOnCreate(shopValidationModel, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();
        try
        {
            var returnedShop = await shopRepository.AddAsync(shop, cancellationToken);
            accountToUpdate.ShopId = returnedShop.Id;
            await accountRepository.UpdateAsync(accountToUpdate, cancellationToken);
            transaction.Commit();

            return mapper.Map<ShopResponse>(returnedShop);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new BadRequestException($"Failed to create shop: {ex.Message}");
        }
    }
}
