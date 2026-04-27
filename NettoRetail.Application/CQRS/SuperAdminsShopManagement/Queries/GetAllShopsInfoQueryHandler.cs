using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.SuperAdminsShopManagement.Queries;

public record GetAllShopsInfoQuery(PaginationRequest PageModel) : IRequest<PaginatedResponse<AllShopsInfoResponse>>;

public class GetAllShopsInfoQueryHandler(
    IShopRepository shopRepository,
    IStaffRepository accountRepository,
    IMapper mapper)
    : IRequestHandler<GetAllShopsInfoQuery, PaginatedResponse<AllShopsInfoResponse>>
{
    public async Task<PaginatedResponse<AllShopsInfoResponse>> Handle(GetAllShopsInfoQuery request, CancellationToken cancellationToken)
    {
        var specification = BuildShopsSpecification(request.PageModel);

        var (shops, totalAmount) = await shopRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var response = mapper.Map<List<AllShopsInfoResponse>>(shops);
        var adminsByShopId = await GetAdminsByShopIdAsync(shops, cancellationToken);

        foreach (var shop in response)
        {
            if (!adminsByShopId.TryGetValue(shop.Id, out var admin))
            {
                shop.AdminFullName = string.Empty;
                shop.AdminEmail = string.Empty;
                shop.AdminPhoneNumber = string.Empty;
                continue;
            }

            shop.AdminFullName = admin.FullName;
            shop.AdminEmail = admin.Email;
            shop.AdminPhoneNumber = admin.PhoneNumber ?? string.Empty;
        }

        return new PaginatedResponse<AllShopsInfoResponse>
        {
            Items = response,
            TotalAmount = totalAmount
        };
    }

    private static Specification<ShopEntity> BuildShopsSpecification(PaginationRequest pageModel)
    {
        var skip = PaginationHelper.CalculateSkip(pageModel.PageNumber, pageModel.PageSize);

        var specificationBuilder = new SpecificationBuilder<ShopEntity>();

        specificationBuilder
            .AddInclude(shop => shop.SubscriptionPlan!)
            .AddOrderBy(shop => shop.Id)
            .Skip(skip)
            .Take(pageModel.PageSize);

        return specificationBuilder.Build();
    }

    private async Task<Dictionary<int, StaffEntity>> GetAdminsByShopIdAsync(
        ICollection<ShopEntity> shops,
        CancellationToken cancellationToken)
    {
        var shopIds = shops
            .Select(shop => shop.Id)
            .Distinct()
            .ToArray();

        if (shopIds.Length == 0)
        {
            return new Dictionary<int, StaffEntity>();
        }

        var adminSpecification = new GetStaffByShopIdsAndRoleSpecification(shopIds, RoleConstants.Admin);
        var admins = await accountRepository.GetManyBySpecAsync(adminSpecification, cancellationToken);

        return admins
            .Where(admin => admin.ShopId.HasValue)
            .GroupBy(admin => admin.ShopId!.Value)
            .ToDictionary(group => group.Key, group => group.OrderBy(admin => admin.DateCreated).First());
    }
}
