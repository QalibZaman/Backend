using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.ClientsShop;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.Application.Services.IShopValidationService;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.SuperAdminsShopManagement.Commands;

public record CreateShopForClientCommand(ClientsShopModel Model) : IRequest<ShopResponse>;

public class CreateShopForClientCommandHandler(IShopRepository shopRepository,
    IStaffRepository accountRepository,
    IUnitOfWork unitOfWork,
    ICountryDetailsValidationService countryDetailsValidator,
    IShopValidationService shopValidator,
    IMapper mapper) : IRequestHandler<CreateShopForClientCommand, ShopResponse>
{
    public async Task<ShopResponse> Handle(CreateShopForClientCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByIdSpecification(request.Model.AccountId);
        var account = await accountRepository.GetAsync(specification, cancellationToken) 
            ?? throw new NotFoundException("Account with such Id not found.");

        if (account.ShopId != null)
        {
            throw new ConflictException("Account already has a shop");
        }

        var countryValidationModel = mapper.Map<CountryValidationModel>(request.Model);
        await countryDetailsValidator.ValidateAsync(countryValidationModel, cancellationToken);

        var shopValidationModel = mapper.Map<ShopValidationModel>(request.Model);
        await shopValidator.ValidateShopOnCreate(shopValidationModel, cancellationToken);

        var shop = mapper.Map<ShopEntity>(request.Model);

        using var transaction = unitOfWork.BeginTransaction();

        await shopRepository.AddAsync(shop, cancellationToken);
        account.ShopId = shop.Id;

        await accountRepository.UpdateAsync(account, cancellationToken);
        transaction.Commit();

        return mapper.Map<ShopResponse>(shop);
    }
}
