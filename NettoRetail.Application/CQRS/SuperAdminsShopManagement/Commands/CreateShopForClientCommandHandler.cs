using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.ClientsShop;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.SuperAdminsShopManagement.Commands;

public record CreateShopForClientCommand(ClientsShopModel Model) : IRequest<ShopResponse>;

public class CreateShopForClientCommandHandler(IShopRepository shopRepository, IAccountRepository accountRepository,
    IUnitOfWork unitOfWork, ICountryDetailsValidationService countryDetailsValidator) : IRequestHandler<CreateShopForClientCommand, ShopResponse>
{
    public async Task<ShopResponse> Handle(CreateShopForClientCommand request, CancellationToken cancellationToken)
    {
        var countryValidationModel = request.Model.Adapt<CountryValidationModel>();

        await countryDetailsValidator.ValidateAsync(countryValidationModel, cancellationToken);

        var shop = request.Model.Adapt<ShopEntity>();
        shop.ShopOwnerId = request.Model.AccountId;

        using var transaction = unitOfWork.BeginTransaction();

        try
        {
            var specification = new GetAccountByIdSpecification(request.Model.AccountId);
            var account = await accountRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("Account with such Id not found.");

            if (account.ShopId != null)
            {
                throw new ConflictException("Account already has a shop");
            }

            await shopRepository.AddAsync(shop, cancellationToken);
            account.ShopId = shop.Id;

            await accountRepository.UpdateAsync(account, cancellationToken);
            transaction.Commit();

            return shop.Adapt<ShopResponse>();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}