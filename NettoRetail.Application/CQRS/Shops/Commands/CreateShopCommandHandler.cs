using System.Text;
using Mapster;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record CreateShopCommand(Guid AccountId, ShopModel Model) : IRequest<ShopResponse>;

public class CreateShopCommandHandler(IShopRepository shopRepository, IAccountRepository accountRepository,
    IUnitOfWork unitOfWork, ICountryDetailsValidationService countryDetailsValidator) : IRequestHandler<CreateShopCommand, ShopResponse>
{
    public async Task<ShopResponse> Handle(CreateShopCommand request, CancellationToken cancellationToken)
    {
        var countryValidationModel = request.Model.Adapt<CountryValidationModel>();

        await countryDetailsValidator.ValidateAsync(countryValidationModel, cancellationToken);

        var shop = request.Model.Adapt<ShopEntity>();

        using var transaction = unitOfWork.BeginTransaction();

        try
        {
            var specification = new GetAccountByIdSpecification(request.AccountId);
            var accountToUpdate = await accountRepository.GetAsync(specification, cancellationToken) ??
                throw new BadRequestException($"Staff member with ID {request.AccountId} not found.");
            if (accountToUpdate.ShopId != null)
            {
                throw new BadRequestException($"Account already has a shop");
            }

            ShopEntity returnedShop;

            var specificationBuilder = new SpecificationBuilder<ShopEntity>();
            specificationBuilder.Add<GetShopByEmailSpecification>(shop.Email)
                .Add<GetShopByNameSpecification>(shop.Name)
                .Add<GetShopByPhoneNumberSpecification>(shop.PhoneNumber);


            var shopsWithRepeatingProps = await shopRepository.GetManyBySpecAsync(specificationBuilder.Build(), cancellationToken);

            if (!shopsWithRepeatingProps.IsNullOrEmpty())
            {
                StringBuilder exceptionMessageBuilder = new StringBuilder();
                foreach (var violatedShop in shopsWithRepeatingProps)
                {
                    if (shop.Name == violatedShop.Name)
                    {
                        exceptionMessageBuilder.AppendLine($"Shop name must be unique");
                    }
                    else if (shop.PhoneNumber == violatedShop.PhoneNumber)
                    {
                        exceptionMessageBuilder.AppendLine($"Shop phone number must be unique");
                    }
                    else if (shop.Email == violatedShop.Email)
                    {
                        exceptionMessageBuilder.AppendLine($"Shop email must be unique");
                    }
                }

                throw new ConflictException(exceptionMessageBuilder.ToString());
            }

            returnedShop = await shopRepository.AddAsync(shop, cancellationToken);

            accountToUpdate.ShopId = returnedShop.Id;

            await accountRepository.UpdateAsync(accountToUpdate, cancellationToken);
            transaction.Commit();

            return returnedShop.Adapt<ShopResponse>();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}