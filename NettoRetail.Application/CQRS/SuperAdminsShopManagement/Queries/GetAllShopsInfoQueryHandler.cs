using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.SuperAdminsShopManagement.Queries;

public record GetAllShopsInfoQuery(PaginationRequest PageModel) : IRequest<PaginatedResponse<AllShopsInfoResponse>>;

public class GetAllShopsInfoQueryHandler(
    IShopRepository shopRepository,
    IAccountRepository accountRepository)
    : IRequestHandler<GetAllShopsInfoQuery, PaginatedResponse<AllShopsInfoResponse>>
{
    public async Task<PaginatedResponse<AllShopsInfoResponse>> Handle(GetAllShopsInfoQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageModel.PageNumber - 1) * request.PageModel.PageSize;

        var specificationBuilder = new SpecificationBuilder<ShopEntity>();
        var specification = specificationBuilder
        .Skip(skip)
        .Take(request.PageModel.PageSize)
        .Build();

        var (shops, totalAmount) = await shopRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var response = shops.Adapt<List<AllShopsInfoResponse>>();

        foreach (var shop in response)
        {
            var adminSpec = new GetAccountByShopIdAndRoleSpecification(shop.Id, RoleConstants.Admin);
            var admin = await accountRepository.GetAsync(adminSpec, cancellationToken);

            shop.AdminFullName = admin != null ? $"{admin.FirstName} {admin.LastName}" : null;
            shop.AdminEmail = admin?.Email;
            shop.AdminPhoneNumber = admin?.PhoneNumber;
        }

        return new PaginatedResponse<AllShopsInfoResponse>
        {
            Items = response,
            TotalAmount = totalAmount
        };
    }
}