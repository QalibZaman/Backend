using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.Application.CQRS.Roles.Queries
{
    public record GetRolesQueryById(int id) : IRequest<RoleResponse>;
    public record GetRolesQueryByName(string Name) : IRequest<RoleResponse>;
    public record GetRolesQuery(PaginationRequest Model) : IRequest<PaginatedResponse<RoleResponse>>;
}
