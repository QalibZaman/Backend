using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Queries;

public record GetAllRolesQuery(int ShopId) : IRequest<IEnumerable<GetAllRolesResponse>>;
public class GetAllRolesQueryHandler(IRoleRepository roleRepository) : IRequestHandler<GetAllRolesQuery, IEnumerable<GetAllRolesResponse>>
{
    public async Task<IEnumerable<GetAllRolesResponse>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetRolesByShopIdSpecification(request.ShopId);
        var roles = await roleRepository.GetAllBySpecAsync(specification, cancellationToken);
        var response = roles.Adapt<IEnumerable<GetAllRolesResponse>>();

        return response;
    }
}
