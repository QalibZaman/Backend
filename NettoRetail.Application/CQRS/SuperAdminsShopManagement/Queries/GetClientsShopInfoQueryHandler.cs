using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.SuperAdminsShopManagement.Queries;

public record GetClientsShopInfoQuery(int ShopId) : IRequest<ClientsShopInfoResponse>;

public class GetClientsShopInfoQueryHandler(
    IShopRepository shopRepository,
    IStaffRepository accountRepository,
    IOptions<AdminRoleOptions> adminOptions,
    IMapper mapper) : IRequestHandler<GetClientsShopInfoQuery, ClientsShopInfoResponse>
{
    public async Task<ClientsShopInfoResponse> Handle(GetClientsShopInfoQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<ShopEntity>()
            .Add<GetShopByIdSpecification>(request.ShopId)
            .AddInclude(x => x.Country)
            .Build();

        var shop = await shopRepository.GetAsync(specificationBuilder, cancellationToken)
            ?? throw new NotFoundException($"Shop with id {request.ShopId} was not found.");

        var getClientAccountByIdSpecification = new GetStaffByShopIdAndRoleSpecification(request.ShopId, adminOptions.Value.RoleId);
        var clientsAccount = await accountRepository.GetAsync(getClientAccountByIdSpecification, cancellationToken)
            ?? throw new NotFoundException("Admin account for this shop was not found.");

        var response = mapper.Map<ClientsShopInfoResponse>(shop);

        mapper.Map(clientsAccount, response);

        return response;
    }

}
