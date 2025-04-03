using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Roles.Queries;

public record GetRolesQuery(GetRolesRequest? Model, int ShopId) : IRequest<PaginatedResponse<RoleResponse>>;

public class GetRolesQueryHandler(IRoleRepository roleRepository)
    : IRequestHandler<GetRolesQuery, PaginatedResponse<RoleResponse>>
{
    public async Task<PaginatedResponse<RoleResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<RoleEntity>();
        var pageSize = request.Model?.PageSize ?? PaginationConstants.DefaultPageSize;
        var skip = request.Model?.PageNumber * pageSize ?? 0;
        var getSpecification = specificationBuilder
            .Add<GetRolesByShopIdSpecification>(request.ShopId)
            .Skip(skip)
            .Take(pageSize)
            .Build()
            .AddInclude(r => r.Accounts);

        var roles = await roleRepository.GetAllPaginatedAsync(getSpecification, cancellationToken);
        var (items, totalAmount) = roles;

        var responseCollection = items.Adapt<List<RoleResponse>>();

        var responseModel = new PaginatedResponse<RoleResponse>
        {
            Items = responseCollection,
            TotalAmount = totalAmount
        };

        return responseModel;
    }
}