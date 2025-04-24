using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.SuperAdminsShopManagement.Queries;

public record GetClientsShopInfoQuery(int ShopId) : IRequest<ClientsShopInfoResponse>;

public class GetClientsShopInfoQueryHandler(IShopRepository shopRepository, IAccountRepository accountRepository) : IRequestHandler<GetClientsShopInfoQuery, ClientsShopInfoResponse>
{
    public async Task<ClientsShopInfoResponse> Handle(GetClientsShopInfoQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<ShopEntity>();
        specificationBuilder.Add<GetShopByIdSpecification>(request.ShopId).AddInclude(x => x.Country);
        
        var shop = await shopRepository.GetAsync(specificationBuilder.Build(), cancellationToken) ?? throw new NotFoundException("There is no shop with such Id");

        var getClientAccountByIdSpecification = new GetAccountByIdSpecification(shop.ShopOwnerId);
        var clientsAccount = await accountRepository.GetAsync(getClientAccountByIdSpecification, cancellationToken) ?? throw new NotFoundException("There is no account with such Id");

        var shopResponse = shop.Adapt<ClientsShopInfoResponse>();
        shopResponse.AdminsEmail = clientsAccount.Email;
        shopResponse.AdminsFullName = $"{clientsAccount.FirstName} {clientsAccount.LastName}";

        return shopResponse;
    }
}