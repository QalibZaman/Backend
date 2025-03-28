using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Queries;

public record GetAllRolesQuery() : IRequest<IEnumerable<GetAllRolesResponse>>;
public class GetAllRolesQueryHandler(IRoleRepository roleRepository) : IRequestHandler<GetAllRolesQuery, IEnumerable<GetAllRolesResponse>>
{
    public async Task<IEnumerable<GetAllRolesResponse>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken);
        var response = roles.Adapt<IEnumerable<GetAllRolesResponse>>();

        return response;
    }
}
