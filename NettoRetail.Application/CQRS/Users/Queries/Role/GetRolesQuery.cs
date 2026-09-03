using MediatR;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.Application.CQRS.Users.Queries.Role
{
    public record GetRolesQueryPaginated(RoleSearchModel Model) : IRequest<PaginatedResponse<RoleResponse>>;

    public record GetRolesQuery() : IRequest<List<RoleResponse>>;
}
