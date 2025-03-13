using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record CreateShopCommand(Guid accountId, ShopModel Model) : IRequest<ShopResponse>;

public class CreateShopCommandHandler(IShopRepository shopRepository, IStaffRepository staffRepository,
    IUnitOfWork unitOfWork, ICountryDetailsValidationService countryDetailsValidator) : IRequestHandler<CreateShopCommand, ShopResponse>
{
    public async Task<ShopResponse> Handle(CreateShopCommand request, CancellationToken cancellationToken)
    {
        await countryDetailsValidator.ValidateAsync(request.Model, cancellationToken);

        var shop = request.Model.Adapt<ShopEntity>();

        using var transaction = unitOfWork.BeginTransaction();

        try
        {
            var specification = new GetStaffByIdSpecification(request.accountId);
            var accountToUpdate = await staffRepository.GetAsync(specification, cancellationToken);
 
            if (accountToUpdate == null)
            {
                throw new BadRequestException($"Staff member with ID {request.accountId} not found.");
            }

            if (accountToUpdate.ShopId != null)
            {
                throw new BadRequestException($"Account already has a shop");
            }

            var returnedShop = await shopRepository.AddAsync(shop, cancellationToken);
            accountToUpdate.ShopId = returnedShop.Id;

            await staffRepository.UpdateAsync(accountToUpdate, cancellationToken);
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
