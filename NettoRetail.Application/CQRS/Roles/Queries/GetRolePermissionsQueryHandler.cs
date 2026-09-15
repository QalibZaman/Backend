using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response.Roles;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Queries;
public record GetRolePermissionQuery(int ShopId) : IRequest<List<GetRolePermissionsResponse>>;
public class GetRolePermissionsQueryHandler(IRoleRepository roleRepository,
    IShopRepository shopRepository,
    IMapper mapper)
    : IRequestHandler<GetRolePermissionQuery, List<GetRolePermissionsResponse>>
{
    public async Task<List<GetRolePermissionsResponse>> Handle(GetRolePermissionQuery request, CancellationToken cancellationToken)
    {
        if (request.ShopId <= 0) throw new BadRequestException("Shop Id cant be 0 or negative");

        var shopExists = await shopRepository.ExistsAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken);
        if (!shopExists) throw new NotFoundException($"No shop with Id: {request.ShopId}");

        var spec = new GetRolesByShopIdSpecification(request.ShopId);
        var roles = await roleRepository.GetRolesWithPermissionsBySpec(spec, cancellationToken);

        var response = mapper.Map<List<GetRolePermissionsResponse>>(roles);

        return response;
    }
}
